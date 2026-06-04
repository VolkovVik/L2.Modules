namespace Aspu.Common.Application.Ports.SignalrPort;

public interface ISignalrNotificationPublisher
{
    Task PublishAsync(Test1Notification notification, CancellationToken cancellationToken = default);

    Task PublishAsync(Test2Notification notification, CancellationToken cancellationToken = default);

    Task PublishAsync(ISignalrNotification notification, CancellationToken cancellationToken = default);
}
