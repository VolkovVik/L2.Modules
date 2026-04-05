using System.Buffers;
using System.Security.Authentication;
using Aspu.Common.Application.Ports.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace Aspu.Common.Infrastructure.Mqtt;

internal sealed class MqttSubscriberHostedService : BackgroundService
{
    private readonly MqttSubscriberOptions _options;
    private readonly MqttConnectionQualityOptions _connectionQuality;
    private readonly IEnumerable<IMqttMessageHandler> _handlers;
    private readonly ILogger<MqttSubscriberHostedService> _logger;
    private CancellationToken _stoppingToken;

    public MqttSubscriberHostedService(
        IOptions<MqttSubscriberOptions> options,
        IEnumerable<IMqttMessageHandler> handlers,
        ILogger<MqttSubscriberHostedService> logger)
    {
        _options = options.Value;
        _connectionQuality = _options.ConnectionQuality ?? new MqttConnectionQualityOptions();
        _handlers = handlers;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;
        var factory = new MqttClientFactory();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var exit = await RunOneSessionAsync(factory, stoppingToken).ConfigureAwait(false);
                if (exit)
                    return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT session error; retrying in {Seconds}s", _options.ReconnectDelaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(_options.ReconnectDelaySeconds), stoppingToken)
                    .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Returns true when the service should exit (empty topics or cancellation).
    /// </summary>
    private async Task<bool> RunOneSessionAsync(MqttClientFactory factory, CancellationToken stoppingToken)
    {
        var subscribeOptions = BuildSubscribeOptions(factory);
        if (subscribeOptions is null)
        {
            _logger.LogWarning("MQTT is enabled but SubscribeTopics is empty; nothing to subscribe.");
            return true;
        }

        var client = factory.CreateMqttClient();
        try
        {
            client.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;

            await client.ConnectAsync(BuildClientOptions(), stoppingToken).ConfigureAwait(false);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("MQTT connected to {Host}:{Port}", _options.Host, _options.Port);

            await client.SubscribeAsync(subscribeOptions, stoppingToken).ConfigureAwait(false);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "MQTT subscribed to topics: {Topics}",
                    string.Join(", ", _options.SubscribeTopics));
            }

            await WaitUntilDisconnectedAsync(client, stoppingToken).ConfigureAwait(false);
            return false;
        }
        finally
        {
            client.ApplicationMessageReceivedAsync -= OnApplicationMessageReceivedAsync;
            try
            {
                await client.DisconnectAsync(new MqttClientDisconnectOptions(), CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "MQTT disconnect after session end");
            }

            client.Dispose();
        }
    }

    private async Task WaitUntilDisconnectedAsync(IMqttClient client, CancellationToken stoppingToken)
    {
        var disconnectCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            if (e.Exception is not null && _logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning(e.Exception, "MQTT disconnected with error");

            disconnectCompletion.TrySetResult();
            return Task.CompletedTask;
        }

        client.DisconnectedAsync += OnDisconnectedAsync;
        try
        {
            await disconnectCompletion.Task.WaitAsync(stoppingToken).ConfigureAwait(false);
        }
        finally
        {
            client.DisconnectedAsync -= OnDisconnectedAsync;
        }
    }

    private MqttClientOptions BuildClientOptions()
    {
        var clientId = string.IsNullOrWhiteSpace(_options.ClientId)
            ? $"aspu-api-{Environment.MachineName}-{Guid.NewGuid():N}"[..48]
            : _options.ClientId;

        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Host, _options.Port)
            .WithClientId(clientId)
            .WithCleanSession(_options.CleanSession);

        if (!string.IsNullOrEmpty(_options.Username))
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

        var built = builder.Build();
        ApplyConnectionQuality(built, _connectionQuality);
        return built;
    }

    private static void ApplyConnectionQuality(MqttClientOptions options, MqttConnectionQualityOptions quality)
    {
        if (quality.KeepAliveSeconds > 0)
            options.KeepAlivePeriod = TimeSpan.FromSeconds(quality.KeepAliveSeconds);

        if (quality.CommunicationTimeoutSeconds > 0)
            options.Timeout = TimeSpan.FromSeconds(quality.CommunicationTimeoutSeconds);
    }

    private MqttClientSubscribeOptions? BuildSubscribeOptions(MqttClientFactory factory)
    {
        var qos = ClampSubscribeQoS(_connectionQuality.SubscribeQualityOfService);
        var subscribeBuilder = factory.CreateSubscribeOptionsBuilder();
        var any = false;
        foreach (var topic in _options.SubscribeTopics)
        {
            if (string.IsNullOrWhiteSpace(topic))
                continue;

            var trimmed = topic.Trim();
            subscribeBuilder = subscribeBuilder.WithTopicFilter(b =>
            {
                switch (qos)
                {
                    case 2:
                        b.WithTopic(trimmed).WithExactlyOnceQoS();
                        break;
                    case 1:
                        b.WithTopic(trimmed).WithAtLeastOnceQoS();
                        break;
                    default:
                        b.WithTopic(trimmed).WithAtMostOnceQoS();
                        break;
                }
            });

            any = true;
        }

        return any ? subscribeBuilder.Build() : null;
    }

    private static int ClampSubscribeQoS(int value) => Math.Clamp(value, 0, 2);

    private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic = e.ApplicationMessage.Topic;
        var payload = PayloadToMemory(e.ApplicationMessage);

        foreach (var handler in _handlers)
        {
            try
            {
                await handler.HandleAsync(topic, payload, _stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(
                        ex,
                        "MQTT handler {Handler} failed for topic {Topic}",
                        handler.GetType().Name,
                        topic);
                }
            }
        }
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
