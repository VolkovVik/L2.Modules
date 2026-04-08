using System.Buffers;
using System.Security.Authentication;
using Microsoft.Extensions.Options;
using MQTTnet;
using Serilog;

namespace Aspu.Common.Infrastructure.Mqtt;

/// <summary>
/// MQTTnet-based subscriber: one connect/subscribe session until disconnect.
/// </summary>
internal sealed class MqttSubscriberClient(IOptions<MqttOptions> Options)
{
    private static readonly MqttClientFactory ClientFactory = new();

    private readonly MqttOptions _options = Options.Value;

    private CancellationToken _sessionCancellationToken;
    private TaskCompletionSource _disconnectCompletion;

    /// <summary>
    /// Runs a single broker session
    /// </summary>
    public async Task RunSessionAsync(CancellationToken cancellationToken)
    {
        _sessionCancellationToken = cancellationToken;
        _disconnectCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var client = ClientFactory.CreateMqttClient();
        try
        {
            var clientOptions = BuildClientOptions();
            var subscribeOptions = BuildSubscribeOptions();

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

    private MqttClientSubscribeOptions? BuildSubscribeOptions()
    {
        var isEnabled = false;
        var subscribeBuilder = ClientFactory.CreateSubscribeOptionsBuilder();

        foreach (var topic in _options.SubscribeTopics
            .Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            isEnabled = true;
            subscribeBuilder.WithTopicFilter(topic.Trim(), _options.QualityOfServiceLevel);
        }

        return !isEnabled ? null : subscribeBuilder.Build();
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
        _ = Task.Run(async () => await ProcessMessageAsync(e.ApplicationMessage, _sessionCancellationToken), _sessionCancellationToken);
        return Task.CompletedTask;
    }

#pragma warning disable S1172 // Unused method parameters should be removed
#pragma warning disable RCS1163 // Unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
    private static Task ProcessMessageAsync(MqttApplicationMessage message, CancellationToken cancellationToken)
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1163 // Unused parameter
#pragma warning restore S1172 // Unused method parameters should be removed
    {
        var topic = message.Topic;
        var sequence = PayloadToMemory(message);

        Log.Debug("MQTT message on {Topic}, payload length {Length}", topic, sequence.Length);
        return Task.CompletedTask;
    }

    private static ReadOnlyMemory<byte> PayloadToMemory(MqttApplicationMessage message)
    {
        var sequence = message.Payload;
        if (sequence.IsEmpty)
            return ReadOnlyMemory<byte>.Empty;

        if (sequence.IsSingleSegment)
            return sequence.First;

        var buffer = new byte[sequence.Length];
        sequence.CopyTo(buffer);
        return buffer;
    }
}
