using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aspu.Modules.Orders.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
#pragma warning disable IDE0060 // Remove unused parameter
        IConfiguration configuration)
#pragma warning restore IDE0060 // Remove unused parameter
     => services;
}
