using Aspu.Api.Extensions.HttpLogging;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Serilog.Events;

BenchmarkRunner.Run<LogFormatterTests>();

[MemoryDiagnoser]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0047:Declare types in namespaces", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1050:Declare types in namespaces", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug", "S3903:Types should be defined in named namespaces", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1110:Declare type inside namespace", Justification = "<Pending>")]
public class LogFormatterTests
{
    private static readonly HttpLogFormatter s_formatter = new();

    private static readonly LogEvent s_logEventNoBody = new(
        DateTimeOffset.Now,
        LogEventLevel.Information,
        exception: null,
        MessageTemplate.Empty,
        [
            new("Protocol", new ScalarValue("HTTP/1.1")),
            new("Method",   new ScalarValue("GET")),
            new("Scheme",   new ScalarValue("http")),
            new("Host",     new ScalarValue("localhost")),
            new("RequestPath", new ScalarValue("/api/")),
            new("User",     new ScalarValue("user")),
            new("StatusCode", new ScalarValue("200")),
            new("Duration", new ScalarValue("1")),
        ]);

    private static readonly LogEvent s_logEventWithBody = new(
        DateTimeOffset.Now,
        LogEventLevel.Information,
        exception: null,
        MessageTemplate.Empty,
        [
            new("Protocol", new ScalarValue("HTTP/1.1")),
            new("Method",   new ScalarValue("POST")),
            new("Scheme",   new ScalarValue("http")),
            new("Host",     new ScalarValue("localhost")),
            new("RequestPath", new ScalarValue("/api/users")),
            new("User",     new ScalarValue("user")),
            new("StatusCode", new ScalarValue("201")),
            new("Duration", new ScalarValue("12")),
            new("RequestBody",  new ScalarValue("{\"id\": 1,\r\n  \"name\": \"John\",\r\n    \"email\": \"john@example.com\"}")),
            new("ResponseBody", new ScalarValue("{\"id\": 1,\r\n  \"name\": \"John\",\r\n    \"email\": \"john@example.com\",\r\n    \"createdAt\": \"2026-01-01T00:00:00Z\"}")),
        ]);

    [Benchmark]
    public void WithoutBody()
    {
        using var textWriter = new StringWriter();
        s_formatter.Format(s_logEventNoBody, textWriter);
    }

    [Benchmark]
    public void WithBody()
    {
        using var textWriter = new StringWriter();
        s_formatter.Format(s_logEventWithBody, textWriter);
    }
}
