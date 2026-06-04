namespace Aspu.Api.Adapters.SignalR;

internal sealed class SignalrMessageWorker(
    SignalrNotificationChannel channel,
    INotificationPublisher notificationPublisher)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var notification in channel.Reader.ReadAllAsync(stoppingToken))
        {
            await notificationPublisher.PublishAsync(notification, stoppingToken);
        }
    }
}
