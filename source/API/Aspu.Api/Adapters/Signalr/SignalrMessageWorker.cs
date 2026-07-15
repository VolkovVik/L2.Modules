using Aspu.Common.SourceGenerators.Application;
using Serilog;

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
        catch (Exception exc)
        {
            Log.Error(exc, "SignalR hosted servise failed");
        }
        finally
        {
            channel.CompleteWriter();
        }
    }

    private async Task DrainRemainingAsync(CancellationToken cancellationToken)
    {
        await foreach (var notification in channel.Reader.ReadAllAsync(cancellationToken))
        {
            await notificationPublisher.PublishAsync(notification, cancellationToken);
        }
    }
}
