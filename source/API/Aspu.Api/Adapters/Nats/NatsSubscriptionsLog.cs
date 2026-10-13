namespace Aspu.Api.Adapters.Nats;

internal static partial class NatsSubscriptionsLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "NATS subscriber has no handlers registered")]
    public static partial void NoHandlers(ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "NATS subscriber ({Mode}) failed; retry in {Seconds} s")]
    public static partial void SubscriberFailed(ILogger logger, Exception exception, string mode, int seconds);

    [LoggerMessage(EventId = 6, Level = LogLevel.Warning, Message = "NATS subscriber ({Mode}) subscription ended; restart in {Seconds} s")]
    public static partial void SubscriptionEnded(ILogger logger, string mode, int seconds);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "NATS inbound queue rejected message on {Topic}; redelivery in {Delay}")]
    public static partial void QueueRejected(ILogger logger, string? topic, TimeSpan delay);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "NATS JetStream message on {Topic} has no payload or subject and is terminated")]
    public static partial void EmptyMessageTerminated(ILogger logger, string? topic);

    [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "NATS inbound queue is full, message on {Topic} is dropped (core NATS has no redelivery)")]
    public static partial void QueueFullDropped(ILogger logger, string topic);
}
