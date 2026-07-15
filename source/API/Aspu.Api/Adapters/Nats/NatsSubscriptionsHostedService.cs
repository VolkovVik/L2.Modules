using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.InboundProcessor;
using Aspu.Common.Presentation.Abstractions.NatsAdapter;
using NATS.Client.Core;
using Serilog;

namespace Aspu.Api.Adapters.Nats;

internal sealed class NatsSubscriptionsHostedService(
    INatsClient client,
    InboundProcessorChannel<NatsOptions> channel,
    InboundProcessorHandlerRegistry<INatsHandler> handlerTopics) :
    BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topics = handlerTopics.GetSubscriptions();
        if (topics.Count == 0)
        {
            Log.Warning("NATS subscriber has no handlers registered");
            return;
        }

        var subscriptions = topics
            .Select(x => client.SubscribeAsync<byte[]>(x, cancellationToken: stoppingToken))
            .ToArray();
        var mergedSubscription = AsyncEnumerableEx.Merge(subscriptions);

        try
        {
            await foreach (var msg in mergedSubscription.WithCancellation(stoppingToken))
            {
                if (msg.Data is null || string.IsNullOrWhiteSpace(msg.Subject))
                    continue;

                var message = new InboundProcessorMessage { Type = "Nats", Topic = msg.Subject, Payload = msg.Data };
                if (!channel.TryEnqueue(message))
                    Log.Warning("NATS inbound queue rejected message on {Topic}", msg.Subject);
            }
        }
        finally
        {
            channel.CompleteWriter();
        }
    }
}
