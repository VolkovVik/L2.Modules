namespace Aspu.Api.Options;

public sealed class NatsOptions
{
    public const string SectionName = "Nats";

    public bool Enabled { get; init; }

    public string Url { get; init; } = "nats://nats:4222";

    public string Name { get; init; } = "aspu-api";

    /// <summary>
    /// Max reconnect attempts. Use -1 for unlimited reconnects.
    /// </summary>
    public int MaxReconnectRetry { get; init; } = -1;

    public int ReconnectWaitMaxSeconds { get; init; } = 1;
}
