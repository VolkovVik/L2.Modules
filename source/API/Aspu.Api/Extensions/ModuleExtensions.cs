using Aspu.Common.Presentation;
using Aspu.Modules.Orders.Presentation;

namespace Aspu.Api.Extensions;

internal static class ModuleExtensions
{
    internal static IServiceCollection AddRequest(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCommonModule();
        services.AddOrdersModule(configuration);
        return services;
    }
}
