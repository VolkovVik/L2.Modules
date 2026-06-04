namespace Aspu.Common.Application.Ports.SignalrPort;

public sealed record Test2Notification(string ErrorId, int Value, DateTime Timestamp) : ISignalrNotification;
