using System.Threading.RateLimiting;
using Aspu.Gateway.Options;
using Microsoft.Extensions.Options;

namespace Aspu.Gateway.Extensions;

internal static class ReverseProxyExtensions
{
    internal static IServiceCollection AddGatewayReverseProxy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GatewayOptions>(configuration.GetSection(GatewayOptions.SectionName));

        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            rateLimiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                if (ShouldBypassRateLimiting(httpContext))
                {
                    return RateLimitPartition.GetNoLimiter("bypass");
                }

                var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromSeconds(10),
                        PermitLimit = 5,
                    });
            });
        });

        services
            .AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));

        return services;
    }

    internal static WebApplication UseGatewayReverseProxy(this WebApplication app)
    {
        app.UseWebSockets(new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromSeconds(30),
        });

        app.UseRateLimiter();
        app.MapReverseProxy();

        return app;
    }

    private static bool ShouldBypassRateLimiting(HttpContext httpContext)
    {
        if (httpContext.WebSockets.IsWebSocketRequest)
        {
            return true;
        }

        if (IsWebSocketUpgradeRequest(httpContext.Request))
        {
            return true;
        }

        var gatewayOptions = httpContext.RequestServices
            .GetRequiredService<IOptions<GatewayOptions>>()
            .Value;

        return httpContext.Request.Path.StartsWithSegments(
            gatewayOptions.SignalRHubPath,
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsWebSocketUpgradeRequest(HttpRequest request) =>
        request.Headers.Connection.ToString().Contains("Upgrade", StringComparison.OrdinalIgnoreCase)
        && request.Headers.Upgrade.ToString().Contains("websocket", StringComparison.OrdinalIgnoreCase);
}
