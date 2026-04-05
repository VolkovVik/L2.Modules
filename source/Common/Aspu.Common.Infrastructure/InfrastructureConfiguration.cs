using Aspu.Common.Application.Ports.Clock;
using Aspu.Common.Application.Ports.Messaging;
using Aspu.Common.Infrastructure.Adapters.Clock;
using Aspu.Common.Infrastructure.Mqtt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aspu.Common.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        var mqttSection = configuration.GetSection(MqttSubscriberOptions.SectionName);
        var mqttOptions = mqttSection.Get<MqttSubscriberOptions>();
        if (mqttOptions?.Enabled == true)
        {
            services.Configure<MqttSubscriberOptions>(mqttSection);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IMqttMessageHandler, LoggingMqttMessageHandler>());
            services.AddHostedService<MqttSubscriberHostedService>();
        }

        return services;
    }
}
