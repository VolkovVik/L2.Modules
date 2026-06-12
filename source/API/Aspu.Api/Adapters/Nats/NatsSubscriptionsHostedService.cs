using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.InboundProcessor;
using NATS.Client.Core;
using Serilog;

namespace Aspu.Api.Adapters.Nats;

internal sealed class NatsSubscriptionsHostedService(
    INatsClient client,
    InboundProcessorChannel<NatsOptions> channel) :
    BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var msg in client.SubscribeAsync<byte[]>(subject: ">", cancellationToken: stoppingToken))
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
