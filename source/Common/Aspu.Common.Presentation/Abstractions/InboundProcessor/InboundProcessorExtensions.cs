using Microsoft.Extensions.DependencyInjection;
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
            sp.GetRequiredService<IServiceScopeFactory>(),
            new TMatcher(),
            sp.GetRequiredService<ILogger<InboundProcessorHandlerRegistry<THandler>>>()));
        // Processor stops after subscriber (reverse registration): subscriber completes the channel writer on exit.
        services.AddHostedService<InboundProcessorHostedService<TOptions, THandler>>();

        return services;
    }
}
