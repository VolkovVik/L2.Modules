using Aspu.Common.Presentation.Abstractions.InboundProcessor;

namespace Aspu.Api.Options;

public sealed class NatsOptions : IInboundProcessorOptions
{
    public const string SectionName = "Nats";

    public bool Enabled { get; init; }

    public string Url { get; init; } = "nats://nats:4222";

    public string Name { get; init; } = "aspu-api";

    /// <summary>
    /// Max reconnect attempts.
    /// Use -1 for unlimited reconnects.
    /// </summary>
    public int MaxReconnectRetry { get; init; } = -1;

    public int ReconnectWaitMaxSeconds { get; init; } = 1;

    /// <summary>
    /// Bounded channel capacity between MQTT receive and the inbound processor.
    /// Minimum effective value is 1.
    /// </summary>
    public int InboundProcessorChannelCapacity { get; init; } = 1024;

    /// <summary>
    /// Max concurrent MQTT inbound message handlers (each message still gets its own DI scope).
    /// Minimum effective value is 1.
    /// </summary>
    public int InboundProcessorMaxDegreeOfParallelism { get; init; } = 16;
}
