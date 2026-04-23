using Aspu.Modules.Orders.Presentation.SourceGenerators.Endpoints;
using Microsoft.Extensions.DependencyInjection;

namespace Aspu.Modules.Orders.Presentation;

public static class PresentationConfiguration
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services)
    {
        services.AddMqttHandlers();

        return services;
    }
}
