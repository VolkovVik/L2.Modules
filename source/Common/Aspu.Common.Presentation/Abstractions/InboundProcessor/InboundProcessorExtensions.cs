using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

public static class InboundProcessorExtensions
{
    public static IServiceCollection AddInboundProcessor<TOptions, THandler, TMatcher>(
        this IServiceCollection services)
        where THandler : IInboundProcessorHandler
        where TOptions : class, IInboundProcessorOptions
        where TMatcher : class, ITopicMatcher, new()
    {
        services.AddSingleton<InboundProcessorChannel<TOptions>>();
        // Matcher is passed directly so NATS and MQTT registries don't share one ITopicMatcher registration.
        services.AddSingleton(sp => new InboundProcessorHandlerRegistry<THandler>(
            sp.GetServices<InboundProcessorTopic<THandler>>(),
            sp.GetRequiredService<IServiceProviderIsKeyedService>(),
            new TMatcher(),
            sp.GetRequiredService<ILogger<InboundProcessorHandlerRegistry<THandler>>>()));
        // Processor stops after subscriber (reverse registration): subscriber completes the channel writer on exit.
        services.AddHostedService<InboundProcessorHostedService<TOptions, THandler>>();

        return services;
    }

    /// <summary>
    /// Registers a scoped handler keyed by its type (resolved per message) and its static <see cref="IInboundTopic.Topic"/> (read by the registry).
    /// Called by the generated <c>Add*Handlers</c> methods; use directly only for handlers outside source generation.
    /// </summary>
    public static IServiceCollection AddInboundProcessorHandler<THandler, TImplementation>(
        this IServiceCollection services)
        where THandler : class, IInboundProcessorHandler
        where TImplementation : class, THandler, IInboundTopic
    {
        services.TryAddKeyedScoped<THandler, TImplementation>(typeof(TImplementation));
        services.AddSingleton(new InboundProcessorTopic<THandler>(TImplementation.Topic, typeof(TImplementation)));

        return services;
    }
}
