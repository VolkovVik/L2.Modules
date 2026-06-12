namespace Aspu.Common.Application.Ports.SignalrPort;

public interface ISignalrNotificationChannel
{
    bool TryWrite(ISignalrNotification notification);
    ValueTask WriteAsync(ISignalrNotification notification, CancellationToken cancellationToken = default);
}
