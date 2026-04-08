using MQTTnet.Protocol;

namespace Aspu.Common.Infrastructure.Mqtt;

public sealed class MqttOptions
{
    public const string SectionName = "Mqtt";

    public bool Enabled { get; init; }

    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 1883;

    public string? ClientId { get; init; }

    public string? Username { get; init; }

    public string? Password { get; init; }

    public bool UseTls { get; init; }

    public bool AllowUntrustedCertificates { get; init; }

    public bool CleanSession { get; init; } = true;

    /// <summary>
    /// MQTT keep-alive interval in seconds. Use 0 to disable (not recommended for long-lived connections).
    /// </summary>
    public int KeepAliveSeconds { get; init; } = 60;

    public int ReconnectDelaySeconds { get; init; } = 5;

    public MqttQualityOfServiceLevel QualityOfServiceLevel { get; set; } = MqttQualityOfServiceLevel.AtLeastOnce;

    public IReadOnlyList<string> SubscribeTopics { get; init; } = [];
}
