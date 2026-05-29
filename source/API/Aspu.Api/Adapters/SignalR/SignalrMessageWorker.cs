using Aspu.Api.Options;
using Microsoft.Extensions.Options;

namespace Aspu.Api.Adapters.SignalR;

internal sealed class SignalrMessageWorker(
    SignalrMessageChannel channel,
    INotificationPublisher notificationPublisher,
    IOptions<SignalROptions> options,
    ILogger<SignalrMessageWorker> logger)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        FlushLoopAsync(stoppingToken);

    private async Task FlushLoopAsync(CancellationToken stoppingToken)
    {
        var signalROptions = options.Value;
        var batchSize = Math.Max(1, signalROptions.BatchSize);
        var flushInterval = TimeSpan.FromMilliseconds(Math.Max(1, signalROptions.FlushIntervalMs));
        var batch = new List<SignalrMessageValue>(batchSize);
        var reader = channel.Reader;

        while (!stoppingToken.IsCancellationRequested)
        {
            batch.Clear();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            cts.CancelAfter(flushInterval);

            try
            {
                while (batch.Count < batchSize)
                {
                    var item = await reader.ReadAsync(cts.Token);
                    batch.Add(item);
                }
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                // Flush interval expired; broadcast whatever was collected.
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (batch.Count > 0)
            {
                await BroadcastBatchAsync(batch, stoppingToken);
            }
        }

        while (reader.TryRead(out var remaining))
        {
            batch.Add(remaining);
        }

        if (batch.Count > 0)
        {
            await BroadcastBatchAsync(batch, CancellationToken.None);
        }
    }

    private async Task BroadcastBatchAsync(
        List<SignalrMessageValue> batch,
        CancellationToken cancellationToken)
    {
        try
        {
            await notificationPublisher.PublishBatchAsync(batch, cancellationToken);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Broadcasted {Count} SignalR messages", batch.Count);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to broadcast {Count} SignalR messages", batch.Count);
        }
    }
}
