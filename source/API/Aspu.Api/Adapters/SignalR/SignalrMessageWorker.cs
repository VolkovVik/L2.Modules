using Aspu.Common.Application.Ports.SignalrPort;

namespace Aspu.Api.Adapters.SignalR;

internal sealed class SignalrMessageWorker(
    SignalrNotificationChannel channel,
    ISignalrNotificationPublisher notificationPublisher)
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
