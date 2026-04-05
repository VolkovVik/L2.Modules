using Aspu.Api.Middleware;

namespace Aspu.Api.Extensions;

internal static class MediatorExtensions
{
    internal static IServiceCollection AddMediatorRequest(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.GenerateTypesAsInternal = true;
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
        });

        return services;
    }
}
