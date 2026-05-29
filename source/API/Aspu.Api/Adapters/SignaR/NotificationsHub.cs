using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace Aspu.Api.Adapters.SignaR;

public interface INotificationsClient
{
    Task ReceiveNotification(string method, object payload, CancellationToken cancellationToken = default);
}

public sealed class NotificationsHub : Hub<INotificationsClient>
{
    public override async Task OnConnectedAsync()
    {
        Log.Debug("SignalR Connected");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Log.Debug("SignalR Disconnected");
        await base.OnDisconnectedAsync(exception);
    }


    public Task SendNotification(string method, object payload, CancellationToken cancellationToken = default) =>
        Clients.All.ReceiveNotification(method, payload, cancellationToken);

    public Task SendNotificationEcho(string content) =>
        Clients.All.ReceiveNotification("ReceiveNotificationEcho", content, Context.ConnectionAborted);
}
