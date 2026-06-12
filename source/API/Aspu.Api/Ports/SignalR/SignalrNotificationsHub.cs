using System.Collections.Concurrent;
using Aspu.Common.SourceGenerators.Application;
using Microsoft.AspNetCore.SignalR;

namespace Aspu.Api.Ports.Signalr;

public sealed class SignalrNotificationsHub(
    ILogger<SignalrNotificationsHub> logger,
    SignalrMetrics metrics) :
    Hub<ISignalrNotificationsHub>
{
    private static readonly ConcurrentDictionary<string, string> _connectionMap = new(StringComparer.Ordinal);

    public override async Task OnConnectedAsync()
    {
        var clientIp = GetClientIp();
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("SignalR client connected: {ConnectionId} {ClientIp}", Context.ConnectionId, clientIp);
        }

        var audience = GetAudience();
        if (!string.IsNullOrWhiteSpace(audience))
            await Groups.AddToGroupAsync(Context.ConnectionId, audience, Context.ConnectionAborted);

        if (!string.IsNullOrWhiteSpace(clientIp))
            _connectionMap.AddOrUpdate(clientIp, Context.ConnectionId, (key, oldValue) => Context.ConnectionId);

        metrics.ClientConnected();

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
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

        var audience = GetAudience();
        if (!string.IsNullOrWhiteSpace(audience))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, audience, Context.ConnectionAborted);

        if (!string.IsNullOrWhiteSpace(clientIp))
            _connectionMap.TryRemove(clientIp, out _);

        metrics.ClientDisconnected();

        await base.OnDisconnectedAsync(exception);
    }

    private string? GetClientIp() =>
        Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

    private string? GetAudience() =>
        Context.GetHttpContext()?.Request.Query["audience"].FirstOrDefault() ?? string.Empty;

    public static string GetConnectionId(string? clientIp) =>
        !string.IsNullOrWhiteSpace(clientIp) &&
        _connectionMap.TryGetValue(clientIp, out var connectionId) &&
        !string.IsNullOrWhiteSpace(connectionId)
            ? connectionId : string.Empty;
}
