using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.InboundProcessor;
using Aspu.Common.Presentation.Abstractions.NatsAdapter;
using Microsoft.Extensions.Options;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using Serilog;

namespace Aspu.Api.Adapters.Nats;

internal sealed class NatsSubscriptionsHostedService(
    INatsClient client,
    INatsJSContext jetStream,
    IOptions<NatsOptions> options,
    InboundProcessorChannel<NatsOptions> channel,
    InboundProcessorHandlerRegistry<INatsHandler> handlerTopics) :
    BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subjects = handlerTopics.GetSubscriptions();
        if (subjects.Count == 0)
        {
            Log.Warning("NATS subscriber has no handlers registered");
            return;
        }

        try
        {
            if (!options.Value.IsJetStreamEnabled)
                await ExecuteInternalAsync(subjects, stoppingToken);
            else
                await JetstreamExecuteInternalAsync(subjects, stoppingToken);

        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // graceful shutdown
        }
        catch (Exception exc)
        {
            Log.Error(exc, "NATS JetStream hosted servise failed");
        }
        finally
        {
            channel.CompleteWriter();
        }
    }

    private async Task ExecuteInternalAsync(IReadOnlyList<string> subjects, CancellationToken cancellationToken)
    {
        var subscriptions = subjects
            .Select(x => client.SubscribeAsync<byte[]>(x, cancellationToken: cancellationToken))
            .ToArray();
        var mergedSubscription = AsyncEnumerableEx.Merge(subscriptions);
        await foreach (var msg in mergedSubscription.WithCancellation(cancellationToken))
        {
            if (msg.Data is null || string.IsNullOrWhiteSpace(msg.Subject))
                continue;

            var message = new InboundProcessorMessage { Type = "Nats", Topic = msg.Subject, Payload = msg.Data };
            if (!channel.TryEnqueue(message))
                Log.Warning("NATS inbound queue rejected message on {Topic}", msg.Subject);
        }
    }

    private async Task JetstreamExecuteInternalAsync(IReadOnlyList<string> subjects, CancellationToken cancellationToken)
    {
        var streamConfig = new StreamConfig
        {
            Name = options.Value.StreamName,
            Subjects = [.. subjects],
            Storage = StreamConfigStorage.File,          // durable: survives a restart
            Retention = StreamConfigRetention.Workqueue, // a queue: acked messages are removed
        };
        await jetStream
            .CreateOrUpdateStreamAsync(streamConfig, cancellationToken)
            .ConfigureAwait(false);

        var consumerConfig = new ConsumerConfig(options.Value.ConsumerName)
        {
            MaxDeliver = 5,                     // drop a poison message after 5 tries
            FilterSubjects = [.. subjects],
            AckWait = TimeSpan.FromSeconds(30), // must exceed your worst-case processing time
            AckPolicy = ConsumerConfigAckPolicy.Explicit,
        };
        var consumer = await jetStream
            .CreateOrUpdateConsumerAsync(options.Value.StreamName, consumerConfig, cancellationToken)
            .ConfigureAwait(false);

        await foreach (var msg in consumer.ConsumeAsync<byte[]>(cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            if (msg.Data is not null && !string.IsNullOrWhiteSpace(msg.Subject))
            {
                var message = new InboundProcessorMessage { Type = "Nats", Topic = msg.Subject, Payload = msg.Data };
                if (channel.TryEnqueue(message))
                {
                    await msg.AckAsync(cancellationToken: cancellationToken).ConfigureAwait(false); // then ack
                    continue;
                }
            }

            Log.Warning("NATS inbound queue rejected message on {Topic}", msg.Subject);
            await msg.NakAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
