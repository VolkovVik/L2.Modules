using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.InboundProcessor;
using Aspu.Common.Presentation.Abstractions.NatsAdapter;
using Microsoft.Extensions.Options;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using Serilog;

namespace Aspu.Api.Adapters.Nats;

internal sealed class NatsJetstreamSubscriptionsHostedService(
    INatsJSContext jetStream,
    IOptions<NatsOptions> options,
    InboundProcessorChannel<NatsOptions> channel,
    InboundProcessorHandlerRegistry<INatsHandler> handlerTopics) :
    BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await ExecuteInternalAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // graceful shutdown
        }
        catch (Exception exc)
        {
            Log.Error(exc, "NATS JetStream hosted servise failed");
        }
    }

    private async Task ExecuteInternalAsync(CancellationToken cancellationToken)
    {
        var topics = handlerTopics.GetSubscriptions().ToArray();
        if (!topics.Any())
        {
            Log.Warning("NATS JetStream subscriber has no handlers registered");
            return;
        }

        var streamConfig = new StreamConfig
        {
            Name = options.Value.StreamName,
            Subjects = topics,
            Storage = StreamConfigStorage.File,          // durable: survives a restart
            Retention = StreamConfigRetention.Workqueue, // a queue: acked messages are removed
        };
        await jetStream
            .CreateOrUpdateStreamAsync(streamConfig, cancellationToken)
            .ConfigureAwait(false);

        var consumerConfig = new ConsumerConfig(options.Value.ConsumerName)
        {
            MaxDeliver = 5,                     // drop a poison message after 5 tries
            FilterSubjects = topics,
            AckWait = TimeSpan.FromSeconds(30), // must exceed your worst-case processing time
            AckPolicy = ConsumerConfigAckPolicy.Explicit,
        };
        var consumer = await jetStream
            .CreateOrUpdateConsumerAsync(options.Value.StreamName, consumerConfig, cancellationToken)
            .ConfigureAwait(false);

        await foreach (var msg in consumer.ConsumeAsync<byte[]>(cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            if (msg.Data is null || string.IsNullOrWhiteSpace(msg.Subject))
            {
                await msg.NakAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                continue;
            }

            var message = new InboundProcessorMessage { Type = "Nats", Topic = msg.Subject, Payload = msg.Data };
            if (channel.TryEnqueue(message))
            {
                await msg.AckAsync(cancellationToken: cancellationToken).ConfigureAwait(false); // then ack
                continue;
            }

            Log.Warning("NATS inbound queue rejected message on {Topic}", msg.Subject);
            await msg.NakAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
