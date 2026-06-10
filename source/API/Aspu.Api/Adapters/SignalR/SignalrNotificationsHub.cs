using System.Collections.Concurrent;
using Aspu.Common.SourceGenerators.Application;
using Microsoft.AspNetCore.SignalR;

namespace Aspu.Api.Adapters.SignalR;

public sealed class SignalrNotificationsHub(
    ILogger<SignalrNotificationsHub> logger) :
    Hub<ISignalrNotificationsHub>
{
    private static readonly ConcurrentDictionary<string, string> _connectionMap = new(StringComparer.Ordinal);

    public override Task OnConnectedAsync()
    {
        var clientIp = GetClientIp();
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("SignalR client connected: {ConnectionId} {ClientIp}", Context.ConnectionId, clientIp);
        }

        if (!string.IsNullOrWhiteSpace(clientIp))
            _connectionMap.AddOrUpdate(clientIp, Context.ConnectionId, (key, oldValue) => Context.ConnectionId);

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var clientIp = GetClientIp();
        if (exception is null)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("SignalR client disconnected: {ConnectionId} {ClientIp}", Context.ConnectionId, clientIp);
        }
        else
        {
            logger.LogWarning(exception, "SignalR client disconnected with error: {ConnectionId} {ClientIp}", Context.ConnectionId, clientIp);
        }

        if (!string.IsNullOrWhiteSpace(clientIp))
            _connectionMap.TryRemove(clientIp, out _);

        return base.OnDisconnectedAsync(exception);
    }

    private string? GetClientIp() =>
        Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

    public static string GetConnectionId(string? clientIp) =>
        !string.IsNullOrWhiteSpace(clientIp) &&
        _connectionMap.TryGetValue(clientIp, out var connectionId) &&
        !string.IsNullOrWhiteSpace(connectionId)
            ? connectionId : string.Empty;
}
