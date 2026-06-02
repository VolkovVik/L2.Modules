using Microsoft.AspNetCore.SignalR;

namespace Aspu.Api.Adapters.SignalR;

public interface INotificationPublisher
{
    Task PublishAsync(SignalrMessage payload, CancellationToken cancellationToken = default);
}

public sealed class SignalRNotificationPublisher(
    IHubContext<NotificationsHub, INotificationsClient> hubContext)
    : INotificationPublisher
{
    public Task PublishAsync(SignalrMessage payload, CancellationToken cancellationToken = default) =>
        hubContext.Clients.All.ReceiveNotification(payload);
}
