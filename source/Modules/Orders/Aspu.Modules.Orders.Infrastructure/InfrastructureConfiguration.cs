using Aspu.Common.Application.Ports.MessageBus;
using Aspu.Modules.Orders.Infrastructure.Adapters.InMemory;
using Aspu.Modules.Orders.Infrastructure.Adapters.Rebus;
using Aspu.Modules.Orders.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aspu.Modules.Orders.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<OrdersMessagingOptions>()
            .Bind(configuration.GetSection(OrdersMessagingOptions.SectionName));

        var messagingOptions = configuration
            .GetSection(OrdersMessagingOptions.SectionName)
            .Get<OrdersMessagingOptions>() ?? new OrdersMessagingOptions();

        if (!messagingOptions.Enabled)
        {
            services.AddSingleton<IIntegrationEventOutbox, NullIntegrationEventOutbox>();
            return services;
        }

        services.AddSingleton<InMemoryOutboxChannel>();
        services.AddOrdersRebus();
        services.AddSingleton<IMessageBus, RebusMessageBus>();
        services.AddSingleton<IIntegrationEventOutbox, InMemoryIntegrationEventOutbox>();
        services.AddHostedService<InMemoryOutboxDispatcherHostedService>();

        return services;
    }
}
