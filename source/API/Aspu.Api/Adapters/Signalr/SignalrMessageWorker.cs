using Aspu.Common.SourceGenerators.Application;

namespace Aspu.Api.Adapters.Signalr;

internal sealed class SignalrMessageWorker(
    SignalrNotificationChannel channel,
    ISignalrNotificationPublisher notificationPublisher)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var notification in channel.Reader.ReadAllAsync(stoppingToken))
            {
                await notificationPublisher.PublishAsync(notification, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            await DrainRemainingAsync(CancellationToken.None);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        channel.CompleteWriter();
        return base.StopAsync(cancellationToken);
    }

    private async Task DrainRemainingAsync(CancellationToken cancellationToken)
    {
        await foreach (var notification in channel.Reader.ReadAllAsync(cancellationToken))
        {
            await notificationPublisher.PublishAsync(notification, cancellationToken);
        }
    }
}
