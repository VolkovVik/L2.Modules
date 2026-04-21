using Aspu.Api.Options;
using Microsoft.Extensions.Options;
using Serilog;

namespace Aspu.Api.Adapters.Mqtt;

/// <summary>
/// Host loop: runs MQTT subscriber sessions with reconnect delay between failures.
/// </summary>
internal sealed class MqttSubscriberHostedService(
    IOptions<MqttOptions> Options,
    MqttSubscriberClient MqttClient,
    MqttMessageHandlerTopicRegistry handlerTopics,
    MqttInboundMessageQueue InboundQueue)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriptions = handlerTopics.SubscriptionTopics();
        if (subscriptions.Count == 0)
            Log.Warning("MQTT subscriber has no valid topic to subscribe");

        var reconnectDelay = Options.Value.ReconnectDelaySeconds;

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await MqttClient.RunSessionAsync(subscriptions, stoppingToken).ConfigureAwait(false);
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
            InboundQueue.CompleteWriter();
        }
    }
}
