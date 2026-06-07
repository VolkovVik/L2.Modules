using Aspu.Common.Application.Ports.SignalrPort;
using Aspu.Common.SourceGenerators.Application;
using Microsoft.AspNetCore.SignalR;

namespace Aspu.Api.Adapters.SignalR;

public sealed class SignalRNotificationPublisher(
    IHubContext<SignalrNotificationsHub, ISignalrNotificationsHub> hubContext)
    : ISignalrNotificationPublisher
{
    public Task PublishAsync(Test1Notification notification, CancellationToken cancellationToken = default) =>
        hubContext.Clients.All.Test1Notification(notification);

    public Task PublishAsync(Test2Notification notification, CancellationToken cancellationToken = default) =>
        hubContext.Clients.All.Test2Notification(notification);

    public Task PublishAsync(ISignalrNotification notification, CancellationToken cancellationToken = default) =>
        notification switch
        {
            Test1Notification test1 => PublishAsync(test1, cancellationToken),
            Test2Notification test2 => PublishAsync(test2, cancellationToken),
            _ => throw new NotSupportedException(
                $"SignalR notification type '{notification.GetType().Name}' is not registered."),
        };
}
