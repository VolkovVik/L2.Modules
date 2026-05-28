using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

/// <summary>
/// Reads messages from the inbound channel and runs <see cref="IInboundProcessorHandler"/> per message in a new DI scope.
/// Processing uses <c>Parallel.ForEachAsync</c> with <see cref="MqttOptions.InboundProcessorMaxDegreeOfParallelism"/>.
/// </summary>
public sealed class InboundProcessorHostedService<TOptions, THandler>(
    IOptions<TOptions> options,
    IServiceScopeFactory scopeFactory,
    InboundProcessorChannel<TOptions> queue,
    InboundProcessorHandlerRegistry<THandler> handlerRegistry,
    ILogger<InboundProcessorHostedService<TOptions, THandler>> logger)
    : BackgroundService
    where THandler : IInboundProcessorHandler
    where TOptions : class, IInboundProcessorOptions
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var maxDop = Math.Max(1, options.Value.InboundProcessorMaxDegreeOfParallelism);
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDop,
            CancellationToken = stoppingToken,
        };

        try
        {
            await Parallel.ForEachAsync(
                    queue.Reader.ReadAllAsync(stoppingToken),
                    parallelOptions,
                    async (item, ct) => await ProcessOneAsync(item, ct).ConfigureAwait(false))
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogTrace(ex, "Inbound processor cancelled with host shutdown");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Inbound processor failed.", ex);
        }
    }

    internal async Task ProcessOneAsync(InboundProcessorMessage item, CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();

        var payload = item.Payload.AsMemory();
        if (payload.IsEmpty || string.IsNullOrWhiteSpace(item.Topic))
            return;

        if (!handlerRegistry.IsEnabled(item.Topic))
        {
            if (logger.IsEnabled(LogLevel.Warning))
                logger.LogWarning("Inbound processor handler for topic {Topic} isn't found", item.Topic);
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var handlers = sp.GetServices<THandler>();
        var handler = handlers.FirstOrDefault(x => string.Equals(x.Topic, item.Topic, StringComparison.OrdinalIgnoreCase));
        if (handler is null)
            return;

        try
        {
            await handler.HandleAsync(item.Topic, payload, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Inbound processor handler for topic {Topic} failed", item.Topic);
        }
        finally
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                var deltaTime = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
                logger.LogInformation("Inbound processor handler on {@Topic} {@Payload} {@Total} ms", item.Topic, Encoding.UTF8.GetString(payload.Span), deltaTime);
            }
        }
    }
}
