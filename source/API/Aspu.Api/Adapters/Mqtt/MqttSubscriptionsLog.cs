namespace Aspu.Api.Adapters.Mqtt;

internal static partial class MqttSubscriptionsLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "MQTT subscriber has no handlers registered")]
    public static partial void NoHandlers(ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "MQTT session ended; reconnect in {Seconds} s")]
    public static partial void SessionEnded(ILogger logger, int seconds);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "MQTT session error; retry in {Seconds} s")]
    public static partial void SessionFailed(ILogger logger, Exception exception, int seconds);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "MQTT connected to {Host}:{Port}")]
    public static partial void Connected(ILogger logger, string host, int port);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "MQTT subscribed to {Count} topics")]
    public static partial void Subscribed(ILogger logger, int count);

    [LoggerMessage(EventId = 6, Level = LogLevel.Warning, Message = "MQTT disconnect after session end failed")]
    public static partial void DisconnectFailed(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "MQTT disconnected")]
    public static partial void Disconnected(ILogger logger);

    [LoggerMessage(EventId = 8, Level = LogLevel.Warning, Message = "MQTT disconnected with error")]
    public static partial void DisconnectedWithError(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 9, Level = LogLevel.Warning, Message = "MQTT disconnected with reason {Reason}")]
    public static partial void DisconnectedWithReason(ILogger logger, string reason);

    [LoggerMessage(EventId = 10, Level = LogLevel.Warning, Message = "MQTT inbound queue rejected message on {Topic}")]
    public static partial void QueueRejected(ILogger logger, string topic);
}
