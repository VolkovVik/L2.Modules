using System.Diagnostics;
using System.Text;
using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.Mqtt;
using Microsoft.Extensions.Options;

namespace Aspu.Api.Adapters.Mqtt;

/// <summary>
/// Reads MQTT messages from the inbound channel and runs <see cref="IMqttMessageHandler"/> per message in a new DI scope.
/// Processing uses <c>Parallel.ForEachAsync</c> with <see cref="MqttOptions.InboundProcessorMaxDegreeOfParallelism"/>.
/// </summary>
internal sealed class MqttInboundMessageHostedService(
    MqttInboundMessageQueue queue,
    IServiceScopeFactory scopeFactory,
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
    private async Task ProcessOneAsync(MqttInboundMessage item, CancellationToken cancellationToken)
    {
        var payload = item.Payload.AsMemory();
        if (string.IsNullOrWhiteSpace(item.Topic) || payload.IsEmpty)
            return;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            var payloadStr = payload.Length <= 100 ? Encoding.UTF8.GetString(payload.Span) : $"[payload length {payload.Length}]";
            logger.LogDebug("Processing MQTT message on topic {Topic} with payload {Payload}", item.Topic, payloadStr);
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var sp = scope.ServiceProvider;

        var handlers = sp.GetServices<IMqttMessageHandler>();
        var handler = handlers.FirstOrDefault(x => string.Equals(x.Topic, item.Topic, StringComparison.OrdinalIgnoreCase));
        if (handler is null)
            return;

        var startTime = Stopwatch.GetTimestamp();

        try
        {
            await handler.HandleAsync(item.Topic, payload, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "MQTT handler failed for topic {Topic}", item.Topic);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MQTT handler cancelled for topic {Topic}", item.Topic);
        }
        finally
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                var deltaTime = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
                logger.LogInformation("Mqtt publication on {@Topic} {@Total} ms", item.Topic, deltaTime);
            }
        }
    }
}
