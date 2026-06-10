namespace Aspu.Common.Application.Ports.SignalrPort;

public interface ISignalrNotification
{
    string? Host { get; }
    DateTime Timestamp { get; }
}
