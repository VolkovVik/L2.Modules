using Microsoft.Extensions.Logging;

namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

internal static partial class InboundProcessorLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Trace, Message = "Inbound processor cancelled with host shutdown")]
    public static partial void Cancelled(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Inbound processor handler pattern for topic {Topic} isn't found")]
    public static partial void PatternNotFound(ILogger logger, string topic);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Inbound processor handler for topic {Topic} isn't found")]
    public static partial void HandlerNotFound(ILogger logger, string topic);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Inbound processor handler {Handler} for topic {Topic} failed")]
    public static partial void HandlerFailed(ILogger logger, Exception exception, string handler, string topic);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "Inbound processor handler on {Topic} {Payload} {Total} ms")]
    public static partial void Processed(ILogger logger, string topic, string payload, double total);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Inbound processor handlers have invalid topic patterns for {Name}: {Handlers}")]
    public static partial void InvalidTopics(ILogger logger, string name, string handlers);

    [LoggerMessage(EventId = 7, Level = LogLevel.Error, Message = "Inbound processor handlers have duplicate topic patterns for {Name}: {Handlers}")]
    public static partial void DuplicateTopics(ILogger logger, string name, string handlers);

    [LoggerMessage(EventId = 8, Level = LogLevel.Error, Message = "Inbound processor handlers have overlapping topic patterns for {Name}: {Handlers}")]
    public static partial void OverlappingTopics(ILogger logger, string name, string handlers);

    [LoggerMessage(EventId = 9, Level = LogLevel.Error, Message = "Inbound processor handlers aren't registered as keyed services for {Name}: {Handlers}")]
    public static partial void NotKeyedHandlers(ILogger logger, string name, string handlers);
}
