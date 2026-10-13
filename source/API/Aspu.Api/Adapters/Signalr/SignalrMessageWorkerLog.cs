namespace Aspu.Api.Adapters.Signalr;

internal static partial class SignalrMessageWorkerLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to publish SignalR notification {NotificationType}")]
    public static partial void PublishFailed(ILogger logger, Exception exception, string notificationType);
}
