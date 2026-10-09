using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.InboundProcessor;
using Aspu.Common.Presentation.Abstractions.MqttAdapter;
using Microsoft.Extensions.Options;

namespace Aspu.Api.Adapters.Mqtt;

/// <summary>
/// Host loop: runs MQTT subscriber sessions with reconnect delay between failures.
/// </summary>
internal sealed class MqttSubscriptionsHostedService(
    IOptions<MqttOptions> options,
    MqttSubscriptionsClient mqttClient,
    InboundProcessorHandlerRegistry<IMqttHandler> handlerTopics,
    InboundProcessorChannel<MqttOptions> channel,
    ILogger<MqttSubscriptionsHostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reconnectDelay = options.Value.ReconnectDelaySeconds;

        try
        {
            if (handlerTopics.IsEmpty)
            {
                MqttSubscriptionsLog.NoHandlers(logger);
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var subscriptions = handlerTopics.GetSubscriptions();
                    await mqttClient.RunSessionAsync(subscriptions, stoppingToken).ConfigureAwait(false);
                    MqttSubscriptionsLog.SessionEnded(logger, reconnectDelay);
                }
                catch (Exception) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exc)
                {
                    MqttSubscriptionsLog.SessionFailed(logger, exc, reconnectDelay);
                }

                await Task.Delay(TimeSpan.FromSeconds(reconnectDelay), stoppingToken)
                    .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
        }
        finally
        {
            channel.CompleteWriter();
        }
    }
}
