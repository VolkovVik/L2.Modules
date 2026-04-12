using Aspu.Api.Options;
using Microsoft.Extensions.Options;
using Serilog;

namespace Aspu.Api.Adapters.Mqtt;

/// <summary>
/// Reads MQTT messages from the inbound channel and runs <see cref="IMqttMessageHandler"/> per message in a new DI scope.
/// Processing uses <c>Parallel.ForEachAsync</c> with <see cref="MqttOptions.InboundProcessorMaxDegreeOfParallelism"/>.
/// </summary>
internal sealed class MqttMessageProcessorHostedService(
    MqttInboundMessageQueue Queue,
    IServiceScopeFactory ScopeFactory,
    IOptions<MqttOptions> MqttOptions,
    ILogger<MqttMessageProcessorHostedService> Logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var maxDop = Math.Max(1, MqttOptions.Value.InboundProcessorMaxDegreeOfParallelism);
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDop,
            CancellationToken = stoppingToken,
        };

        try
        {
            await Parallel.ForEachAsync(
                    Queue.Reader.ReadAllAsync(stoppingToken),
                    parallelOptions,
                    async (item, ct) => await ProcessOneAsync(item, ct).ConfigureAwait(false))
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (stoppingToken.IsCancellationRequested)
        {
            Logger.LogTrace(ex, "MQTT inbound processor cancelled with host shutdown");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("MQTT inbound processor failed.", ex);
        }
    }

#pragma warning disable S1172 // Unused method parameters should be removed
#pragma warning disable RCS1163 // Unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
    private async Task ProcessOneAsync(MqttInboundMessage item, CancellationToken cancellationToken)
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1163 // Unused parameter
#pragma warning restore S1172 // Unused method parameters should be removed
    {
        await using var scope = ScopeFactory.CreateAsyncScope();

#pragma warning disable RCS1139 // Add summary element to documentation comment
#pragma warning disable S1481 // Unused local variables should be removed
        var sp = scope.ServiceProvider;

        /// var handlers = sp.GetServices<IMqttMessageHandler>();
        var payload = item.Payload.AsMemory();

        Log.Information("MQTT processed message on {Topic}", item.Topic);

        /// foreach (var handler in handlers)
        /// {
        ///     try
        ///     {
        ///         await handler.HandleAsync(item.Topic, payload, cancellationToken).ConfigureAwait(false);
        ///     }
        ///     catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        ///     {
        ///         Logger.LogError(ex, "MQTT handler failed for topic {Topic}", item.Topic);
        ///     }
        /// }
#pragma warning restore S1481 // Unused local variables should be removed
#pragma warning restore RCS1139 // Add summary element to documentation comment
    }
}
