using Aspu.Modules.Orders.Application;
using Aspu.Modules.Orders.Infrastructure;
using Aspu.Modules.Orders.Presentation.SourceGenerators.Endpoints;
using Aspu.Modules.Orders.Presentation.SourceGenerators.Validators;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aspu.Modules.Orders.Presentation;

public static class OrdersConfiguration
{
    public static IServiceCollection AddOrdersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);

        services.AddValidators();

        return services;
    }

    public static IEndpointRouteBuilder MapOrdersEndpoints(
        this IEndpointRouteBuilder app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        EndpointsRegistration.MapEndpoints(app, routeGroupBuilder);

        return routeGroupBuilder ?? app;
    }
}
