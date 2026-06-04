namespace Aspu.Common.Application.Ports.SignalrPort;

public interface ISignalrNotificationChannel
{
    ValueTask WriteAsync(ISignalrNotification notification, CancellationToken cancellationToken = default);
}
