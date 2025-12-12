using System.Globalization;
using Serilog.Events;
using Serilog.Formatting;

namespace Aspu.Api.Extensions.HttpLogging;

public class HttpLogFormatter() : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(output);

        output.Write(logEvent.Timestamp.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture));
        output.Write(" ");
        output.Write(AbbreviateLogLevel(logEvent.Level));
        output.Write(" ");
        output.Write(FormatEvent(logEvent, "Protocol"));
        output.Write(" ");
        output.Write(FormatEvent(logEvent, "Method"));
        output.Write(" ");
        output.Write($"{FormatEvent(logEvent, "Scheme")}://{FormatEvent(logEvent, "Host")}{FormatEvent(logEvent, "RequestPath")}");

        var user = FormatEvent(logEvent, "User");
        if (!string.IsNullOrWhiteSpace(user))
        {
            output.Write(" ");
            output.Write($"<user={user}>");
        }

        output.Write(" ");
        output.Write("responded ");
        output.Write(FormatEvent(logEvent, "StatusCode"));
        output.Write(" in ");
        output.Write($"{FormatEvent(logEvent, "Duration")} ms");
        output.WriteLine();

        var requestBody = TransformBody(FormatEvent(logEvent, "RequestBody"));
        if (!string.IsNullOrWhiteSpace(requestBody))
        {
            output.Write($"request - {requestBody}");
            output.WriteLine();
        }
        var responseBody = TransformBody(FormatEvent(logEvent, "ResponseBody"));
        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            output.Write($"response - {responseBody}");
            output.WriteLine();
        }
    }

    private static string FormatEvent(LogEvent logEvent, string name) =>
        logEvent.Properties.TryGetValue(name, out var value) ? value.ToString().Trim('"') : string.Empty;

    private static string TransformBody(string body) =>
        body.Replace("\\\"", "\"")
            .Replace("\r\n    ", string.Empty)
            .Replace("\r\n  ", string.Empty)
            .Replace("\r\n", string.Empty)
            .Replace("\n    ", string.Empty)
            .Replace("\n  ", string.Empty)
            .Replace("\n", string.Empty)
            .Trim('"');

    private static string AbbreviateLogLevel(LogEventLevel level) =>
        level switch
        {
            LogEventLevel.Verbose => "[VRB]",
            LogEventLevel.Debug => "[DBG]",
            LogEventLevel.Information => "[INF]",
            LogEventLevel.Warning => "[WRN]",
            LogEventLevel.Error => "[ERR]",
            LogEventLevel.Fatal => "[FTL]",
            _ => level.ToString().ToUpper(CultureInfo.InvariantCulture)[..3] // Fallback
        };
}
