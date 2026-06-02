namespace Aspu.Common.Presentation.Abstractions.SignalR;

public interface ISignalrNotificationChannel
{
    ValueTask WriteAsync(ISignalrNotification notification, CancellationToken cancellationToken = default);
}
