namespace Aspu.Api.Adapters.Nats;

internal static partial class NatsSubscriptionsLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "NATS subscriber has no handlers registered")]
    public static partial void NoHandlers(ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "NATS subscriber ({Mode}) failed")]
    public static partial void SubscriberFailed(ILogger logger, Exception exception, string mode);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "NATS inbound queue rejected message on {Topic}")]
    public static partial void QueueRejected(ILogger logger, string? topic);
}
