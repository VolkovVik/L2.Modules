using System.Diagnostics;
using Microsoft.AspNetCore.HttpLogging;

namespace Aspu.Api.Extensions.HttpLogging;

public class CustomLoggingInterceptor : IHttpLoggingInterceptor
{
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
        // Example: Remove specific headers from being logged
        logContext.HttpContext.Request.Headers.Remove("X-API-Key");

        // Example: Add custom information to log
        var traceId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
        logContext.AddParameter("TraceId", traceId);

        var user = logContext.HttpContext.User?.Identity?.Name ?? "-";
        logContext.AddParameter("User", user);

        if (!string.IsNullOrWhiteSpace(logContext.HttpContext.Request.Path.Value) &&
            (logContext.HttpContext.Request.Path.Value.Contains("scalar") ||
            logContext.HttpContext.Request.Path.Value.Contains("openapi")))
            logContext.LoggingFields = HttpLoggingFields.Request | HttpLoggingFields.Duration;

        return ValueTask.CompletedTask;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
    {
        // Example: Remove sensitive response header
        logContext.HttpContext.Response.Headers.Remove("Set-Cookie");

        return ValueTask.CompletedTask;
    }
}
