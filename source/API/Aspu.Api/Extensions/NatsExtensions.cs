using Aspu.Api.Adapters.Nats;
using Aspu.Api.Options;
using Microsoft.Extensions.Options;
using NATS.Extensions.Microsoft.DependencyInjection;

namespace Aspu.Api.Extensions;

internal static class NatsExtensions
{
    internal static IServiceCollection AddNatsSubscriber(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var natsOptions = configuration.GetSection(NatsOptions.SectionName).Get<NatsOptions>();
        if (natsOptions?.Enabled != true)
            return services;

        services.AddConfiguredNatsClient();

        services.AddHostedService<SoilAnalyticsWorker>();
        services.AddHostedService<SoilAnalyticsWorker1>();
        services.AddHostedService<SoilAnalyticsWorker2>();
        services.AddHostedService<SoilAnalyticsWorker3>();
        services.AddHostedService<SoilAnalyticsWorker4>();
        services.AddHostedService<IrrigationDeviceWorker>();
        services.AddHostedService<IrrigationControllerWorker>();

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
