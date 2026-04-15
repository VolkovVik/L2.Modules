using Aspu.Common.Presentation.Abstractions.Mqtt;
using Aspu.Modules.Orders.Application;
using Aspu.Modules.Orders.Infrastructure;
using Aspu.Modules.Orders.Presentation.Adapters.Mqtt;
using Aspu.Modules.Orders.Presentation.SourceGenerators.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aspu.Modules.Orders.Presentation;

public static class OrdersConfiguration
{
    public static IServiceCollection AddOrdersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddPresentation();

        return services;
    }

    public static IEndpointRouteBuilder MapOrdersEndpoints(
        this IEndpointRouteBuilder app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        EndpointsRegistration.MapEndpoints(app, routeGroupBuilder);

        return routeGroupBuilder ?? app;
    }

    private static IServiceCollection AddPresentation(
        this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IMqttMessageHandler, MqttAddCodeHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IMqttMessageHandler, OrdersAddCodeHandler1>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IMqttMessageHandler, OrdersAddCodeHandler2>());

        return services;
    }
}
