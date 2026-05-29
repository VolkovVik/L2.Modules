using Aspu.Api.Extensions.Subscriptions;
using Aspu.Api.SourceGenerators.Application;
using Aspu.Api.SourceGenerators.Presentation;

namespace Aspu.Api;

public static class ApiConfiguration
{
    public static IServiceCollection AddApiModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidators();
        services.AddSignalRSubscriber();
        services.AddMqttSubscriber(configuration);
        services.AddNatsSubscriber(configuration);

        return services;
    }

    public static IEndpointRouteBuilder MapApiEndpoints(
        this IEndpointRouteBuilder app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        MapHelloEndpoint(app);
        HttpEndpointsRegistration.MapHttpEndpoints(app, routeGroupBuilder);

        return routeGroupBuilder ?? app;
    }

    private static void MapHelloEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/", () => "Hello from ASPU.API")
            .AllowAnonymous()
            .WithName("Hello")
            .WithSummary("Hello")
            .WithDescription("Return hello message")
            .WithTags("Hello");
}
