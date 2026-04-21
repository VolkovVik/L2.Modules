using System.Buffers;
using System.Security.Authentication;
using Aspu.Api.Options;
using Microsoft.Extensions.Options;
using MQTTnet;
using Serilog;

namespace Aspu.Api.Adapters.Mqtt;

/// <summary>
/// MQTTnet-based subscriber: one connect/subscribe session until disconnect.
/// </summary>
internal sealed class MqttSubscriberClient(
    IOptions<MqttOptions> options,
    MqttInboundMessageQueue inboundQueue)
{
    private static readonly MqttClientFactory ClientFactory = new();

    private readonly MqttOptions _options = options.Value;

    private TaskCompletionSource _disconnectCompletion;

    /// <summary>
    /// Runs a single broker session
    /// </summary>
    public async Task RunSessionAsync(IReadOnlyList<string> subscriptions, CancellationToken cancellationToken)
    {
        _disconnectCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var client = ClientFactory.CreateMqttClient();
        try
        {
            var clientOptions = BuildClientOptions();
            var subscribeOptions = BuildSubscribeOptions(subscriptions);

            client.DisconnectedAsync += OnClientDisconnectedAsync;
            client.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;

            await client.ConnectAsync(clientOptions, cancellationToken)
                .ConfigureAwait(false);

            Log.Information("MQTT connected to {Host}:{Port}", _options.Host, _options.Port);

            if (subscribeOptions is not null)
            {
                await client.SubscribeAsync(subscribeOptions, cancellationToken)
                    .ConfigureAwait(false);

                Log.Information("MQTT subscribed succesfully");
            }

            await _disconnectCompletion.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

        }
        catch (Exception exc)
        {
            Log.Error(exc, "MQTT exception");
        }
        finally
        {
            client.ApplicationMessageReceivedAsync -= OnApplicationMessageReceivedAsync;
            client.DisconnectedAsync -= OnClientDisconnectedAsync;
            try
            {
                await client.DisconnectAsync(new MqttClientDisconnectOptions(), CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "MQTT disconnect after session end");
            }

            client.Dispose();

            Log.Information("MQTT disconnected");
        }
    }

    private MqttClientSubscribeOptions? BuildSubscribeOptions(IReadOnlyList<string> subscriptions)
    {
        if (!subscriptions.Any())
            return null;

        var subscribeBuilder = ClientFactory.CreateSubscribeOptionsBuilder();
        foreach (var topic in subscriptions)
            subscribeBuilder.WithTopicFilter(topic, _options.QualityOfServiceLevel);

        return subscribeBuilder.Build();
    }

    private MqttClientOptions BuildClientOptions()
    {
        var clientId = string.IsNullOrWhiteSpace(_options.ClientId)
            ? $"aspu-subscriber-{Guid.NewGuid():N}"
            : _options.ClientId;

        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Host, _options.Port)
            .WithClientId(clientId)
            .WithCleanSession(_options.CleanSession);

        builder = _options.KeepAliveSeconds > 0
            ? builder.WithKeepAlivePeriod(TimeSpan.FromSeconds(_options.KeepAliveSeconds))
            : builder.WithNoKeepAlive();

        if (!string.IsNullOrWhiteSpace(_options.Username))
            builder = builder.WithCredentials(_options.Username, _options.Password ?? string.Empty);

        if (_options.UseTls)
        {
            builder = builder.WithTlsOptions(o =>
            {
                o.WithSslProtocols(SslProtocols.None);
                if (_options.AllowUntrustedCertificates)
                    o.WithCertificateValidationHandler(_ => true);
            });
        }

        return builder.Build();
    }

    private Task OnClientDisconnectedAsync(MqttClientDisconnectedEventArgs e)
    {
        if (e.Exception is not null)
            Log.Warning(e.Exception, "MQTT disconnected with error");

        if (!string.IsNullOrWhiteSpace(e.ReasonString))
            Log.Warning("MQTT disconnected with reason {@Reason}", e.ReasonString);

        _disconnectCompletion.TrySetResult();

        return Task.CompletedTask;
    }

    private Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var message = e.ApplicationMessage;
        var topic = message.Topic;
        Log.Information("MQTT received message on {Topic}", topic);
        var payload = PayloadToOwnedBuffer(message);

        var inbound = new MqttInboundMessage { Topic = topic, Payload = payload };
        if (!inboundQueue.TryEnqueue(inbound))
            Log.Warning("MQTT inbound queue rejected message on {Topic}", topic);

        return Task.CompletedTask;
    }

    private static byte[] PayloadToOwnedBuffer(MqttApplicationMessage message)
    {
        var sequence = message.Payload;
        if (sequence.IsEmpty)
            return [];

        if (sequence.IsSingleSegment)
            return sequence.First.ToArray();

        var buffer = new byte[sequence.Length];
        sequence.CopyTo(buffer.AsSpan());
        return buffer;
    }
}
