using Aspu.Api.Adapters.Nats;
using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.InboundProcessor;
using Aspu.Common.Presentation.Abstractions.NatsAdapter;
using Microsoft.Extensions.Options;
using NATS.Extensions.Microsoft.DependencyInjection;

namespace Aspu.Api.Extensions.Subscriptions;

internal static class NatsExtensions
{
    internal static IServiceCollection AddNatsSubscriber(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(NatsOptions.SectionName).Get<NatsOptions>();
        if (options?.Enabled != true)
            return services;

        services.AddConfiguredNatsClient();
        services.AddInboundProcessor<NatsOptions, INatsHandler>();
        services.AddHostedService<NatsSubscriptionsHostedService>();

        return services;
    }

    private static void AddConfiguredNatsClient(
        this IServiceCollection services)
    {
        services.AddNatsClient(builder =>
        {
            builder.ConfigureOptions((OptionsBuilder<NatsOptsBuilder> optionsBuilder) =>
            {
                optionsBuilder.Configure<IOptions<NatsOptions>>((natsOptsBuilder, natsOptions) =>
                {
                    var options = natsOptions.Value;

                    natsOptsBuilder.Opts = natsOptsBuilder.Opts with
                    {
                        Url = options.Url,
                        Name = options.Name,
                        MaxReconnectRetry = options.MaxReconnectRetry,
                        ReconnectWaitMax = TimeSpan.FromSeconds(options.ReconnectWaitMaxSeconds),
                    };
                });
            });
        });
    }
}
