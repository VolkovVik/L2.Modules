namespace Aspu.Api.Options;

public sealed class SignalROptions
{
    public const string SectionName = "SignalR";

    public bool Enabled { get; init; } = true;

    public string HubPath { get; init; } = "/notifications-hub";

    /// <summary>
    /// Bounded channel capacity between producers and the batch flush worker.
    /// Minimum effective value is 1.
    /// </summary>
    public int ChannelCapacity { get; init; } = 10_000;

    /// <summary>
    /// Max items per SignalR broadcast batch.
    /// Minimum effective value is 1.
    /// </summary>
    public int BatchSize { get; init; } = 1_000;

    /// <summary>
    /// Max delay before a partial batch is flushed, in milliseconds.
    /// Minimum effective value is 1.
    /// </summary>
    public int FlushIntervalMs { get; init; } = 50;

    public bool SingleReader { get; init; } = true;

    public bool SingleWriter { get; init; }
}
