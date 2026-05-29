using Serilog;

namespace Aspu.Api.Adapters.SignaR;

internal class SignalrMessageWorker(
    SignalrMessageChannel channel,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<SignalrMessageWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await FlushLoopAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while executing SignalrMessageWorker");
        }
    }

    private async Task FlushLoopAsync(CancellationToken stoppingToken)
    {
        const int BatchSize = 10;

        var batch = new List<SignalrMessageValue>(BatchSize);
        var reader = channel.Reader;

        while (!stoppingToken.IsCancellationRequested)
        {
            batch.Clear();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            cts.CancelAfter(TimeSpan.FromMilliseconds(60_000));

            try
            {
                while (batch.Count < BatchSize)
                {
                    var item = await reader.ReadAsync(cts.Token);
                    batch.Add(item);
                }
            }
            catch (OperationCanceledException)
            {
                // Either the flush interval expired or the service is stopping.
                // In both cases, flush whatever we have collected so far.
            }

            if (batch.Count > 0)
            {
                await BroadcastBatchAsync(batch);
            }
        }

        // Drain remaining items after cancellation
        while (reader.TryRead(out var remaining))
        {
            batch.Add(remaining);
        }

        if (batch.Count > 0)
        {
            await BroadcastBatchAsync(batch);
        }
    }

    private async Task BroadcastBatchAsync(List<SignalrMessageValue> batch)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var publisher = scope.ServiceProvider.GetRequiredService<INotificationPublisher>();
            await publisher.PublishAsync("ReceiveDataPoints", batch, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to broadcast {Count} data points", batch.Count);
        }
    }
}
