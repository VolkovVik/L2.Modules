using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace Aspu.Api.Extensions.HttpLogging;

public class DynamicFormatter() : ITextFormatter
{
    private const string MiddlewareName = "\"Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware\"";

    private static readonly ITextFormatter _specialFormatter = new HttpLogFormatter();
    private static readonly MessageTemplateTextFormatter _defaultFormatter = new("{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

    public void Format(LogEvent logEvent, TextWriter output)
    {
        var context = logEvent.Properties.TryGetValue("SourceContext", out var sourceContext)
            ? sourceContext.ToString() : null;

        var formatter = !string.IsNullOrWhiteSpace(context) &&
            string.Equals(context, MiddlewareName, StringComparison.OrdinalIgnoreCase)
            ? _specialFormatter
            : _defaultFormatter;
        formatter.Format(logEvent, output);
    }
}
