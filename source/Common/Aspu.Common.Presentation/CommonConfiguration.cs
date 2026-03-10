using System.Reflection;
using Aspu.Common.Application;
using Aspu.Common.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Aspu.Common.Presentation;

public static class CommonConfiguration
{
    public static IServiceCollection AddCommonModule(
        this IServiceCollection services,
#pragma warning disable IDE0060 // Remove unused parameter
        Assembly[] moduleAssemblies)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        services.AddApplication();
        services.AddInfrastructure();

#pragma warning disable IDE0022 // Use expression body for method
        return services;
#pragma warning restore IDE0022 // Use expression body for method
    }
}
