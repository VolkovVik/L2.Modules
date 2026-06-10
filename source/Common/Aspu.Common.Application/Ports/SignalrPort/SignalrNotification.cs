namespace Aspu.Common.Application.Ports.SignalrPort;

public abstract record SignalrNotification(DateTime Timestamp, string? Host = null) : ISignalrNotification;
