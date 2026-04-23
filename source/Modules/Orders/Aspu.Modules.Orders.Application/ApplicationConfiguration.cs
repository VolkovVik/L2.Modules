using Aspu.Modules.Orders.Application.SourceGenerators.Validators;
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
