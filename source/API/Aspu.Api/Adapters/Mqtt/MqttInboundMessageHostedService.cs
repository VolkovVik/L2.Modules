using System.Diagnostics;
using System.Text;
using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.MqttAdapter;
using Microsoft.Extensions.Options;

namespace Aspu.Api.Adapters.Mqtt;

/// <summary>
/// Reads MQTT messages from the inbound channel and runs <see cref="IMqttHandler"/> per message in a new DI scope.
/// Processing uses <c>Parallel.ForEachAsync</c> with <see cref="MqttOptions.InboundProcessorMaxDegreeOfParallelism"/>.
/// </summary>
internal sealed class MqttInboundMessageHostedService(
    MqttInboundMessageQueue queue,
    IServiceScopeFactory scopeFactory,
    MqttHandlerTopicRegistry handlerTopics,
    IOptions<MqttOptions> mqttOptions,
    ILogger<MqttInboundMessageHostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var maxDop = Math.Max(1, mqttOptions.Value.InboundProcessorMaxDegreeOfParallelism);
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
            logger.LogTrace(ex, "MQTT inbound processor cancelled with host shutdown");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("MQTT inbound processor failed.", ex);
        }
    }
    internal async Task ProcessOneAsync(MqttInboundMessage item, CancellationToken cancellationToken)
    {
        var payload = item.Payload.AsMemory();
        if (payload.IsEmpty || string.IsNullOrWhiteSpace(item.Topic))
            return;

        if (!handlerTopics.IsTopicEnabled(item.Topic))
        {
            if (logger.IsEnabled(LogLevel.Warning))
                logger.LogWarning("MQTT handler for topic {Topic} isn't found", item.Topic);
            return;
        }

        var startTime = Stopwatch.GetTimestamp();

        await using var scope = scopeFactory.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var handlers = sp.GetServices<IMqttHandler>();

        var handler = handlers.FirstOrDefault(x => string.Equals(x.Topic, item.Topic, StringComparison.OrdinalIgnoreCase));
        if (handler is null)
            return;

        try
        {
            await handler.HandleAsync(item.Topic, payload, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "MQTT handler for topic {Topic} failed", item.Topic);
        }
        finally
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                var deltaTime = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
                logger.LogInformation("MQTT publication on {@Topic} {@Payload} {@Total} ms", item.Topic, Encoding.UTF8.GetString(payload.Span), deltaTime);
            }
        }
    }
}
