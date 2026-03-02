using Aspu.Api.Middleware;

namespace Aspu.Api.Extensions;

internal static class MediatorExtensions
{
    internal static IServiceCollection AddRequest(this IServiceCollection services)
    {
        services.AddMediator((Mediator.MediatorOptions options) =>
        {
            options.GenerateTypesAsInternal = true;
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
        });

        SourceGeneratorsLibrary.ValidatorRegistrationGenerator.AddValidators(services);

        return services;
    }
}
