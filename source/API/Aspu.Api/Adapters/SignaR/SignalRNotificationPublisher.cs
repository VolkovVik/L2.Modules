using Microsoft.AspNetCore.SignalR;

namespace Aspu.Api.Adapters.SignaR;

public interface INotificationPublisher
{
    Task PublishAsync(string method, object payload, CancellationToken cancellationToken = default);
}

public sealed record SignalrMessageValue(string Method, object Payload, DateTime Timestamp);

public class SignalRNotificationPublisher(
    IHubContext<NotificationsHub, INotificationsClient> hubContext)
    : INotificationPublisher
{
    public Task PublishAsync(string method, object payload, CancellationToken cancellationToken = default) =>
        hubContext.Clients.All.ReceiveNotification(method, payload, cancellationToken);
}
