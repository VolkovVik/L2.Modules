using Aspu.Common.Application;
using Aspu.Common.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aspu.Common.Presentation;

public static class CommonConfiguration
{
    public static IServiceCollection AddCommonModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        return services;
    }
}
