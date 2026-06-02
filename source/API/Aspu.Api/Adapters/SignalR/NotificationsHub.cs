using Microsoft.AspNetCore.SignalR;

namespace Aspu.Api.Adapters.SignalR;

public sealed class NotificationsHub(ILogger<NotificationsHub> logger) :
    Hub<INotificationsClient>
{
    public override Task OnConnectedAsync()
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("SignalR client connected: {ConnectionId}", Context.ConnectionId);
        }

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is null)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("SignalR client disconnected: {ConnectionId}", Context.ConnectionId);
            }
        }
        else
        {
            logger.LogWarning(
                exception,
                "SignalR client disconnected with error: {ConnectionId}",
                Context.ConnectionId);
        }

        return base.OnDisconnectedAsync(exception);
    }
}
