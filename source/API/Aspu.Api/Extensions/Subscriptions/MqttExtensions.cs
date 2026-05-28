using Aspu.Api.Adapters.Mqtt;
using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.InboundProcessor;
using Aspu.Common.Presentation.Abstractions.MqttAdapter;

namespace Aspu.Api.Extensions.Subscriptions;

internal static class MqttExtensions
{
    internal static IServiceCollection AddMqttSubscriber(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(MqttOptions.SectionName).Get<MqttOptions>();
        if (options?.Enabled != true)
            return services;

        services.AddSingleton<MqttSubscriptionsClient>();
        services.AddInboundProcessor<MqttOptions, IMqttHandler>();
        services.AddHostedService<MqttSubscriptionsHostedService>();

        return services;
    }
}
