using Aspu.Modules.Orders.SourceGenerators.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Aspu.Modules.Orders.Application;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidators();

        return services;
    }
}
