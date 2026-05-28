using Microsoft.Extensions.DependencyInjection;

namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

public static class InboundProcessorExtensions
{
    public static IServiceCollection AddInboundProcessor<TOptions, THandler>(
        this IServiceCollection services)
        where THandler : IInboundProcessorHandler
        where TOptions : class, IInboundProcessorOptions
    {
        services.AddSingleton<InboundProcessorChannel<TOptions>>();
        services.AddSingleton<InboundProcessorHandlerRegistry<THandler>>();
        // Processor stops after subscriber (reverse registration): subscriber completes the channel writer on exit.
        services.AddHostedService<InboundProcessorHostedService<TOptions, THandler>>();

        return services;
    }
}
