using Microsoft.Extensions.Logging;

namespace Aspu.Modules.Orders.Application.UseCases.Codes;

// EventIds continue AddCodeLogger (13) so module log events stay distinct.
internal static partial class CodeNotificationLog
{
    [LoggerMessage(EventId = 14, Level = LogLevel.Debug, Message = "Add code notification {Id} handled")]
    public static partial void AddCodeHandled(ILogger logger, Guid id);

    [LoggerMessage(EventId = 15, Level = LogLevel.Debug, Message = "Get code notification {Id} handled")]
    public static partial void GetCodeHandled(ILogger logger, Guid id);

    [LoggerMessage(EventId = 16, Level = LogLevel.Debug, Message = "Get code by id notification {Id} handled")]
    public static partial void GetCodeByIdHandled(ILogger logger, Guid id);
}
