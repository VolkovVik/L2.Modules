using Aspu.Api.Extensions.HttpLogging;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Serilog.Events;

BenchmarkRunner.Run<LogFormatterTests>();

// dotnet build -c Release
// dotnet run -c Release

[MemoryDiagnoser]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1050:Declare types in namespaces", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug", "S3903:Types should be defined in named namespaces", Justification = "<Pending>")]
public class LogFormatterTests
{
    [Benchmark]
    public void WithSpanBuilder()
    {
        using var textWriter = new StringWriter();

        var properties = new List<LogEventProperty>()
        {
            new ("Protocol", new ScalarValue("HTTP/1.1")),
            new ("Method", new ScalarValue("GET")),
            new ("Scheme", new ScalarValue("http")),
            new ("Host", new ScalarValue("localhost")),
            new ("RequestPath", new ScalarValue("/api/")),
            new ("User", new ScalarValue("user")),
            new ("StatusCode", new ScalarValue("200")),
            new ("Duration", new ScalarValue("1")),
        };
        var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, MessageTemplate.Empty, properties);

        var formatter = new HttpLogFormatter();

        formatter.Format(logEvent, textWriter);
    }
}
