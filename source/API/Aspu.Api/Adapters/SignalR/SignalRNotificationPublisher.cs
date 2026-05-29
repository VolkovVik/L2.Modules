using Microsoft.AspNetCore.SignalR;

namespace Aspu.Api.Adapters.SignalR;

public interface INotificationPublisher
{
    Task PublishAsync(string method, object payload, CancellationToken cancellationToken = default);
}

public sealed class SignalRNotificationPublisher(
    IHubContext<NotificationsHub, INotificationsClient> hubContext)
    : INotificationPublisher
{
    public Task PublishAsync(string method, object payload, CancellationToken cancellationToken = default) =>
        hubContext.Clients.All.ReceiveNotification(method, payload);
}
