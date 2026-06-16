using Aspu.Common.Application.Ports.MessageBus;
using Aspu.Modules.Orders.Infrastructure.Adapters.Postgres;
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
            .Bind(configuration.GetSection(OrdersMessagingOptions.SectionName))
            .Validate(
                options => !options.Enabled || !string.IsNullOrWhiteSpace(options.ConnectionString),
                "Orders:Messaging:ConnectionString is required when messaging is enabled.")
            .ValidateOnStart();

        var messagingOptions = configuration
            .GetSection(OrdersMessagingOptions.SectionName)
            .Get<OrdersMessagingOptions>() ?? new OrdersMessagingOptions();

        if (!messagingOptions.Enabled)
        {
            services.AddSingleton<IIntegrationEventOutbox, NullIntegrationEventOutbox>();
            return services;
        }

        services.AddOrdersRebus();
        services.AddSingleton<IMessageBus, RebusMessageBus>();
        services.AddSingleton<IIntegrationEventOutbox, IntegrationEventOutbox>();
        services.AddHostedService<OutboxSchemaInitializer>();
        services.AddHostedService<OutboxDispatcherHostedService>();

        return services;
    }
}
