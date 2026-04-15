using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.Mqtt;
using Microsoft.Extensions.Options;
using Serilog;

namespace Aspu.Api.Adapters.Mqtt;

/// <summary>
/// Host loop: runs MQTT subscriber sessions with reconnect delay between failures.
/// </summary>
internal sealed class MqttSubscriberHostedService(
    IOptions<MqttOptions> Options,
    MqttSubscriberClient MqttClient,
    IServiceScopeFactory scopeFactory,
    MqttInboundMessageQueue InboundQueue)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriptions = GetSubscriptions();
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

    private List<string> GetSubscriptions()
    {
        using var scope = scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;
        var handlers = sp.GetServices<IMqttMessageHandler>();

        var topics = handlers
            .Select(h => h.Topic.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();
        if (topics.Count == 0)
            Log.Warning("MQTT subscriber has no valid topic to subscribe");
        return topics;
    }
}
