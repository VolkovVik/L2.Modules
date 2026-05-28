using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.InboundProcessor;
using Aspu.Common.Presentation.Abstractions.MqttAdapter;
using Microsoft.Extensions.Options;
using Serilog;

namespace Aspu.Api.Adapters.Mqtt;

/// <summary>
/// Host loop: runs MQTT subscriber sessions with reconnect delay between failures.
/// </summary>
internal sealed class MqttSubscriptionsHostedService(
    IOptions<MqttOptions> options,
    MqttSubscriptionsClient mqttClient,
    InboundProcessorHandlerRegistry<IMqttHandler> handlerTopics,
    InboundProcessorChannel<MqttOptions> channel)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriptions = handlerTopics.GetSubscriptions();
        if (subscriptions.Count == 0)
            Log.Warning("MQTT subscriber has no valid topic to subscribe");

        var reconnectDelay = options.Value.ReconnectDelaySeconds;

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await mqttClient.RunSessionAsync(subscriptions, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exc)
                {
                    Log.Error(exc, "MQTT session error; retry in {@Seconds} s", reconnectDelay);
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(reconnectDelay), stoppingToken).ConfigureAwait(false);
                }
            }
        }
        finally
        {
            channel.CompleteWriter();
        }
    }
}
