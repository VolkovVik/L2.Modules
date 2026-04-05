namespace Aspu.Common.Infrastructure.Mqtt;

public sealed class MqttSubscriberOptions
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

    public int ReconnectDelaySeconds { get; init; } = 5;

    public MqttConnectionQualityOptions ConnectionQuality { get; init; } = new();

    public IReadOnlyList<string> SubscribeTopics { get; init; } = [];
}
