using Aspu.Common.Application;
using Aspu.Common.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Aspu.Common.Presentation;

public static class CommonConfiguration
{
    public static IServiceCollection AddCommonModule(
        this IServiceCollection services)
    {
        services.AddApplication();
        services.AddInfrastructure();
        return services;
    }
}
