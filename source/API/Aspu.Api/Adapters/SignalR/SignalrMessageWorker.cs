namespace Aspu.Api.Adapters.SignalR;

internal sealed class SignalrMessageWorker(
    SignalrMessageChannel channel,
    INotificationPublisher notificationPublisher)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in channel.Reader.ReadAllAsync(stoppingToken))
            await notificationPublisher.PublishAsync(message.Method, message.Payload, stoppingToken);
    }
}
