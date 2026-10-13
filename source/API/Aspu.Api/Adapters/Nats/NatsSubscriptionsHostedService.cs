using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.InboundProcessor;
using Aspu.Common.Presentation.Abstractions.NatsAdapter;
using Microsoft.Extensions.Options;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace Aspu.Api.Adapters.Nats;

internal sealed class NatsSubscriptionsHostedService(
    INatsClient client,
    INatsJSContext jetStream,
    IOptions<NatsOptions> options,
    InboundProcessorChannel<NatsOptions> channel,
    InboundProcessorHandlerRegistry<INatsHandler> handlerTopics,
    ILogger<NatsSubscriptionsHostedService> logger) :
    BackgroundService
{
    /// <summary>
    /// Host loop: restarts the subscription after a failure or an unexpected end, like the MQTT subscriber,
    /// so a single error doesn't stop NATS intake until the application restarts.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var mode = options.Value.IsJetStreamEnabled ? "JetStream" : "Core";
        var retryDelay = Math.Max(1, options.Value.SubscriptionRetryDelaySeconds);

        try
        {
            if (handlerTopics.IsEmpty)
            {
                NatsSubscriptionsLog.NoHandlers(logger);
                return;
            }

            var subjects = handlerTopics.GetSubscriptions();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!options.Value.IsJetStreamEnabled)
                        await ExecuteInternalAsync(subjects, stoppingToken).ConfigureAwait(false);
                    else
                        await JetstreamExecuteInternalAsync(subjects, stoppingToken).ConfigureAwait(false);

                    NatsSubscriptionsLog.SubscriptionEnded(logger, mode, retryDelay);
                }
                catch (Exception) when (stoppingToken.IsCancellationRequested)
                {
                    break; // graceful shutdown
                }
                catch (Exception exc)
                {
                    NatsSubscriptionsLog.SubscriberFailed(logger, exc, mode, retryDelay);
                }

                await Task.Delay(TimeSpan.FromSeconds(retryDelay), stoppingToken)
                    .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
        }
        finally
        {
            // Completed only when the host stops, so the inbound processor keeps running between retries.
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

            var message = new InboundProcessorMessage { Topic = msg.Subject, Payload = msg.Data };
            if (!channel.TryEnqueue(message))
                NatsSubscriptionsLog.QueueFullDropped(logger, msg.Subject);
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

        var redeliveryDelay = TimeSpan.FromSeconds(Math.Max(1, options.Value.JetStreamRedeliveryDelaySeconds));

        await foreach (var msg in consumer.ConsumeAsync<byte[]>(cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            // Redelivery can't fix a message without payload or subject (the processor drops it anyway),
            // so it's terminated instead of being nak'ed up to MaxDeliver times.
            if (msg.Data is not { Length: > 0 } || string.IsNullOrWhiteSpace(msg.Subject))
            {
                NatsSubscriptionsLog.EmptyMessageTerminated(logger, msg.Subject);
                await msg.AckTerminateAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                continue;
            }

            var message = new InboundProcessorMessage { Topic = msg.Subject, Payload = msg.Data };
            if (channel.TryEnqueue(message))
            {
                await msg.AckAsync(cancellationToken: cancellationToken).ConfigureAwait(false); // then ack
                continue;
            }

            // Delayed nak: an immediate one would burn all MaxDeliver attempts while the queue is still full.
            NatsSubscriptionsLog.QueueRejected(logger, msg.Subject, redeliveryDelay);
            await msg.NakAsync(delay: redeliveryDelay, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
