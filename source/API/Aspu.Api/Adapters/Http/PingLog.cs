namespace Aspu.Api.Adapters.Http;

internal static partial class PingLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Ping {Id} started")]
    public static partial void Started(ILogger logger, Guid id);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Ping {Id} publishing notification")]
    public static partial void Publishing(ILogger logger, Guid id);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Ping {Id} finished")]
    public static partial void Finished(ILogger logger, Guid id);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Ping notification {Id} handled")]
    public static partial void NotificationHandled(ILogger logger, Guid id);
}
