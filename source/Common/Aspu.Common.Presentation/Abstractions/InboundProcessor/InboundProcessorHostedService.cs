using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

/// <summary>
/// Reads messages from the inbound channel and runs the single <see cref="IInboundProcessorHandler"/> whose topic pattern matches
/// (patterns can't overlap, see <see cref="InboundProcessorHandlerRegistry{THandler}"/>), resolved as a keyed service in a new DI scope per message.
/// Handler failures are logged and don't stop processing.
/// Processing uses <c>Parallel.ForEachAsync</c> with <see cref="IInboundProcessorOptions.InboundProcessorMaxDegreeOfParallelism"/>.
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
            InboundProcessorLog.Cancelled(logger, ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Inbound processor failed.", ex);
        }
    }

    public async Task ProcessOneAsync(InboundProcessorMessage item, CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();

        var payload = item.Payload.AsMemory();
        if (payload.IsEmpty || string.IsNullOrWhiteSpace(item.Topic))
            return;

        if (!handlerRegistry.TryResolve(item.Topic, out var handlerType))
        {
            InboundProcessorLog.PatternNotFound(logger, item.Topic);
            return;
        }

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetKeyedService<THandler>(handlerType);
            if (handler is null)
            {
                InboundProcessorLog.HandlerNotFound(logger, item.Topic);
                return;
            }

            await handler.HandleAsync(item.Topic, payload, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception ex)
        {
            InboundProcessorLog.HandlerFailed(logger, ex, handlerType.Name, item.Topic);
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            var payloadString = Encoding.UTF8.GetString(payload.Span);
            var delay = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
            InboundProcessorLog.Processed(logger, item.Topic, payloadString, delay);
        }
    }
}
