using Aspu.Common.Presentation;
using Aspu.Modules.Orders.Presentation;

namespace Aspu.Api.Extensions;

internal static class ModuleExtensions
{
    internal static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApiModule();
        services.AddCommonModule();
        services.AddOrdersModule(configuration);

        return services;
    }
}
