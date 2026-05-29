namespace Aspu.Gateway.Options;

public sealed class GatewayOptions
{
    public const string SectionName = "Gateway";

    /// <summary>
    /// SignalR hub path on the upstream API. Must match <c>SignalR:HubPath</c> in Aspu.Api.
    /// </summary>
    public string SignalRHubPath { get; init; } = "/notifications-hub";
}
