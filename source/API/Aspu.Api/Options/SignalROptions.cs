namespace Aspu.Api.Options;

public sealed class SignalROptions
{
    public const string SectionName = "SignalR";

    public bool Enabled { get; init; } = true;

    public string HubPath { get; init; } = "/notifications-hub";

    /// <summary>
    /// Bounded channel capacity between producers and the SignalR message worker.
    /// Minimum effective value is 1.
    /// </summary>
    public int ChannelCapacity { get; init; } = 10_000;

    public bool SingleReader { get; init; } = true;

    public bool SingleWriter { get; init; }
}
