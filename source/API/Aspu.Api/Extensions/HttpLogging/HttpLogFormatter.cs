using System.Buffers;
using System.Globalization;
using Serilog.Events;
using Serilog.Formatting;

namespace Aspu.Api.Extensions.HttpLogging;

public sealed class HttpLogFormatter() : ITextFormatter
{
    private const int TimestampBufferSize = 32;
    private const int ScalarNumberBufferSize = 64;
    private const string TimePattern = "yyyy-MM-dd HH:mm:ss.fff zzz";

    private static readonly CultureInfo UsedCultureInfo = CultureInfo.InvariantCulture;

    public void Format(LogEvent logEvent, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(output);

        WriteTimestamp(output, logEvent.Timestamp);
        output.Write(' ');
        WriteLogLevel(output, logEvent.Level);
        output.Write(' ');
        WriteProperty(output, logEvent, "Protocol");
        output.Write(' ');
        WriteProperty(output, logEvent, "Method");
        output.Write(' ');
        WriteProperty(output, logEvent, "Scheme");
        output.Write("://");
        WriteProperty(output, logEvent, "Host");
        WriteProperty(output, logEvent, "RequestPath");
        WriteProperty(output, logEvent, "User", " <user=", ">");
        output.Write(" responded ");
        WriteProperty(output, logEvent, "StatusCode");
        output.Write(" in ");
        WriteProperty(output, logEvent, "Duration");
        output.Write(" ms");
        output.WriteLine();
        WriteBodyProperty(output, logEvent, "RequestBody", "request - ");
        WriteBodyProperty(output, logEvent, "ResponseBody", "response - ");
    }

    private static void WriteTimestamp(TextWriter output, DateTimeOffset timestamp)
    {
        Span<char> buffer = stackalloc char[TimestampBufferSize];
        var utcTimestamp = timestamp.UtcDateTime;
        if (utcTimestamp.TryFormat(buffer, out var written, TimePattern, UsedCultureInfo))
        {
            output.Write(buffer[..written]);
            return;
        }

        output.Write(utcTimestamp.ToString(TimePattern, UsedCultureInfo));
    }

    private static void WriteLogLevel(TextWriter output, LogEventLevel level)
    {
        var span = level switch
        {
            LogEventLevel.Verbose => "[VRB]".AsSpan(),
            LogEventLevel.Debug => "[DBG]".AsSpan(),
            LogEventLevel.Information => "[INF]".AsSpan(),
            LogEventLevel.Warning => "[WRN]".AsSpan(),
            LogEventLevel.Error => "[ERR]".AsSpan(),
            LogEventLevel.Fatal => "[FTL]".AsSpan(),
            _ => default,
        };
        if (!span.IsEmpty)
        {
            output.Write(span);
            return;
        }

        var s = level.ToString().ToUpper(UsedCultureInfo);
        output.Write('[');
        output.Write(s.Length > 3 ? s.AsSpan(0, 3) : s.AsSpan());
        output.Write(']');
    }

    private static void WriteProperty(TextWriter output, LogEvent logEvent, string name, string prefix = "", string postfix = "")
    {
        if (!logEvent.Properties.TryGetValue(name, out var value))
            return;

        Span<char> buffer = stackalloc char[ScalarNumberBufferSize];
        var span = TryFormatScalar(value, buffer);
        if (span.IsEmpty)
            return;

        if (prefix.Length > 0)
            output.Write(prefix);

        output.Write(span);

        if (postfix.Length > 0)
            output.Write(postfix);
    }

    private static void WriteBodyProperty(TextWriter output, LogEvent logEvent, string name, string prefix)
    {
        if (!logEvent.Properties.TryGetValue(name, out var value))
            return;

        var span = value is ScalarValue { Value: string s }
            ? s.AsSpan()
            : value.ToString(format: null, UsedCultureInfo).AsSpan();
        span = TrimQuotes(span);
        if (span.IsEmpty)
            return;

        output.Write(prefix);
        WriteCompactBody(output, span);
        output.WriteLine();
    }

    private static ReadOnlySpan<char> TryFormatScalar(LogEventPropertyValue value, Span<char> buffer)
    {
        if (value is ScalarValue { Value: string s })
            return TrimQuotes(s.AsSpan());

        if (value is not ScalarValue { Value: var scalarValue })
            return TrimQuotes(value.ToString(format: null, UsedCultureInfo).AsSpan());

        var written = 0;
        var result = scalarValue switch
        {
            byte number => number.TryFormat(buffer, out written, provider: UsedCultureInfo),
            sbyte number => number.TryFormat(buffer, out written, provider: UsedCultureInfo),
            short number => number.TryFormat(buffer, out written, provider: UsedCultureInfo),
            ushort number => number.TryFormat(buffer, out written, provider: UsedCultureInfo),
            int number => number.TryFormat(buffer, out written, provider: UsedCultureInfo),
            uint number => number.TryFormat(buffer, out written, provider: UsedCultureInfo),
            long number => number.TryFormat(buffer, out written, provider: UsedCultureInfo),
            ulong number => number.TryFormat(buffer, out written, provider: UsedCultureInfo),
            nint number => number.TryFormat(buffer, out written, provider: UsedCultureInfo),
            nuint number => number.TryFormat(buffer, out written, provider: UsedCultureInfo),
            float number => number.TryFormat(buffer, out written, provider: UsedCultureInfo),
            double number => number.TryFormat(buffer, out written, provider: UsedCultureInfo),
            decimal number => number.TryFormat(buffer, out written, provider: UsedCultureInfo),
            _ => false,
        };
        return result ? buffer[..written] : [];
    }

    private static ReadOnlySpan<char> TrimQuotes(ReadOnlySpan<char> span)
    {
        span = !span.IsEmpty && span[0] is '"' ? span[1..] : span;
        return !span.IsEmpty && span[^1] is '"' ? span[..^1] : span;
    }

    private static readonly SearchValues<char> s_specialBodyChars = SearchValues.Create("\\\r\n");

    private static void WriteCompactBody(TextWriter output, ReadOnlySpan<char> span)
    {
        while (!span.IsEmpty)
        {
            var idx = span.IndexOfAny(s_specialBodyChars);
            if (idx < 0)
            {
                output.Write(span);
                return;
            }

            if (idx > 0)
                output.Write(span[..idx]);

            var c = span[idx];
            span = span[(idx + 1)..];

            if (!span.IsEmpty && c is '\\' && span[0] is '"')
            {
                output.Write('"');
                span = span[1..];
                continue;
            }
            if (c is '\r' or '\n')
            {
                while (!span.IsEmpty && span[0] is ' ' or '\t' or '\r' or '\n')
                    span = span[1..];

                continue;
            }

            output.Write(c);
        }
    }
}
