namespace Aspu.Common.Application.Ports.SignalrPort;

public sealed record Test1Notification(string Description) : SignalrNotification(DateTime.UtcNow);
