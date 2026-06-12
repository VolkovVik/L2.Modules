namespace Aspu.Common.Application.Ports.SignalrPort;

public sealed record Test2Notification(string ErrorId, int Value) : SignalrNotification(DateTime.UtcNow);

public sealed record Test3Notification(string ErrorId, int Value) : SignalrNotification(DateTime.UtcNow);
