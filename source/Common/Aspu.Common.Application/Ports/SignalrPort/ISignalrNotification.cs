namespace Aspu.Common.Application.Ports.SignalrPort;

public interface ISignalrNotification
{
    string? Host { get; }
    string? Audience { get; }
    DateTime Timestamp { get; }
}
