using Aspu.Common.Presentation.Abstractions.InboundProcessor;

namespace Aspu.Api.Options;

public sealed class NatsOptions : IInboundProcessorOptions
{
    public const string SectionName = "Nats";

    public bool IsEnabled { get; init; }

    public bool IsJetStreamEnabled { get; init; }

    public string Url { get; init; } = "nats://nats:4222";

    public string Name { get; init; } = "aspu-api";

    public string StreamName { get; set; } = "aspu-stream";

    public string ConsumerName { get; set; } = "aspu-consumer";

    /// <summary>
    /// Max reconnect attempts.
    /// Use -1 for unlimited reconnects.
    /// </summary>
    public int MaxReconnectRetry { get; init; } = -1;

    public int ReconnectWaitMaxSeconds { get; init; } = 1;

    /// <summary>
    /// Delay before the subscriber restarts after its subscription failed or ended
    /// (stream/consumer setup error, consumer deleted, unexpected exception).
    /// Connection drops are handled by the NATS client itself (<see cref="ReconnectWaitMaxSeconds"/>).
    /// </summary>
    public int SubscriptionRetryDelaySeconds { get; init; } = 5;

    /// <summary>
    /// JetStream: delay before a message rejected by the full inbound queue is redelivered.
    /// Without it the consumer's MaxDeliver attempts are spent within milliseconds and the message is dropped.
    /// </summary>
    public int JetStreamRedeliveryDelaySeconds { get; init; } = 5;

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
