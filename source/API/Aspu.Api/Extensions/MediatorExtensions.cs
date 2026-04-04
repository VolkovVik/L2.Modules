using Aspu.Api.Middleware;

namespace Aspu.Api.Extensions;

internal static class MediatorExtensions
{
    internal static IServiceCollection AddRequest(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.GenerateTypesAsInternal = true;
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
        });

        SourceGenerators.Validators.ValidatorRegistration.AddValidators(services);
        Modules.Orders.Presentation.SourceGenerators.Validators.ValidatorRegistration.AddValidators(services);

        return services;
    }
}
