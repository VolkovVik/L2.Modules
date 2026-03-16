using System.Diagnostics;
using Microsoft.AspNetCore.HttpLogging;

namespace Aspu.Api.Extensions.HttpLogging;

public sealed class CustomLoggingInterceptor : IHttpLoggingInterceptor
{
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
        var httpContext = logContext.HttpContext;
        var request = httpContext.Request;
        var path = request.Path.Value;

        request.Headers.Remove("X-API-Key");

        var traceId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
        logContext.AddParameter("TraceId", traceId);

        var user = httpContext.User?.Identity?.Name ?? "-";
        logContext.AddParameter("User", user);
        logContext.AddParameter("Host", request.Host.Value ?? string.Empty);

        if (!string.IsNullOrWhiteSpace(path) &&
            (path.Contains("scalar", StringComparison.OrdinalIgnoreCase) ||
             path.Contains("openapi", StringComparison.OrdinalIgnoreCase)))
        {
            logContext.LoggingFields = HttpLoggingFields.Request | HttpLoggingFields.Duration;
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
    {
        logContext.HttpContext.Response.Headers.Remove("Set-Cookie");

        return ValueTask.CompletedTask;
    }
}
