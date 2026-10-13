using Aspu.Common.Application.Ports.SignalrPort;
using Aspu.Common.SourceGenerators.Application;

namespace Aspu.Api.Adapters.Signalr;

internal sealed class SignalrMessageWorker(
    SignalrNotificationChannel channel,
    ISignalrNotificationPublisher notificationPublisher,
    ILogger<SignalrMessageWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var notification in channel.Reader.ReadAllAsync(stoppingToken))
            {
                await PublishSafeAsync(notification, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Graceful shutdown: remaining notifications are drained in finally.
        }
        finally
        {
            channel.CompleteWriter();
            await DrainRemainingAsync();
        }
    }

    private async Task DrainRemainingAsync()
    {
        while (channel.Reader.TryRead(out var notification))
        {
            await PublishSafeAsync(notification, CancellationToken.None);
        }
    }

    private async Task PublishSafeAsync(ISignalrNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await notificationPublisher.PublishAsync(notification, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exc)
        {
            SignalrMessageWorkerLog.PublishFailed(logger, exc, notification.GetType().Name);
        }
    }
}
