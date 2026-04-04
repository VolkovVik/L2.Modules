using Aspu.Api.SourceGenerators.Endpoints;
using Aspu.Api.SourceGenerators.Validators;

namespace Aspu.Api;

public static class ApiConfiguration
{
    public static IServiceCollection AddApiModule(
        this IServiceCollection services)
    {
        services.AddValidators();

        return services;
    }

    public static IEndpointRouteBuilder MapApiEndpoints(
        this IEndpointRouteBuilder app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        MapHelloEndpoint(app);
        EndpointsRegistration.MapEndpoints(app, routeGroupBuilder);

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
