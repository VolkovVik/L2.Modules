using Aspu.Common.Application.Ports.SignalrPort;

namespace Aspu.Api.Adapters.SignalR;

public interface INotificationPublisher
{
    Task PublishAsync(Test1Notification notification, CancellationToken cancellationToken = default);

    Task PublishAsync(Test2Notification notification, CancellationToken cancellationToken = default);

    Task PublishAsync(ISignalrNotification notification, CancellationToken cancellationToken = default);
}
