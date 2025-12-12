using Microsoft.AspNetCore.HttpLogging;

namespace Aspu.Api.Extensions.HttpLogging;

internal static class HttpLoggingExtensions
{
    ///<remarks>
    /// HTTP logging in ASP.NET Core
    /// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-logging/?view=aspnetcore-9.0
    /// https://antondevtips.com/blog/logging-requests-and-responses-for-api-requests-and-httpclient-in-aspnetcore
    ///</remarks>

    internal static WebApplicationBuilder AddHttpRequestLogging(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpLogging(options =>
        {
            options.CombineLogs = true;

            options.LoggingFields =
                HttpLoggingFields.RequestProtocol |
                HttpLoggingFields.RequestScheme |
                HttpLoggingFields.RequestMethod |
                HttpLoggingFields.RequestPath |
                HttpLoggingFields.RequestBody |
                HttpLoggingFields.RequestQuery |
                HttpLoggingFields.RequestHeaders |
                HttpLoggingFields.ResponseBody |
                HttpLoggingFields.ResponseHeaders |
                HttpLoggingFields.ResponseStatusCode |
                HttpLoggingFields.Duration;

            options.RequestBodyLogLimit = 1024 * 32; // 32 KB
            options.ResponseBodyLogLimit = 1024 * 32; // 32 KB

            if (builder.Environment.IsDevelopment())
                options.LoggingFields |= HttpLoggingFields.RequestHeaders | HttpLoggingFields.ResponseHeaders;
        });

        builder.Services.AddHttpLoggingInterceptor<CustomLoggingInterceptor>();

        return builder;
    }
}
