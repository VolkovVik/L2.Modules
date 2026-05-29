namespace Aspu.Api.Adapters.SignalR;

public sealed record SignalrMessage(string Method, object Payload, DateTime Timestamp);
