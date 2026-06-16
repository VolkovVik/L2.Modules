using Aspu.Modules.Orders.Infrastructure.Adapters.Rebus.Handlers;
using Aspu.Modules.Orders.Infrastructure.Options;
using Aspu.Modules.Orders.IntegrationEvents.Codes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rebus.Config;
using Rebus.PostgreSql;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;

namespace Aspu.Modules.Orders.Infrastructure.Adapters.Rebus;

internal static class RebusServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersRebus(this IServiceCollection services)
    {
        services.AddRebus(
            (configure, provider) =>
            {
                var options = provider.GetRequiredService<IOptions<OrdersMessagingOptions>>().Value;

                return configure
                    .Transport(t => t.UsePostgreSql(
                        options.ConnectionString,
                        options.TransportTable,
                        options.InputQueue))
                    .Subscriptions(s => s.StoreInPostgres(
                        options.ConnectionString,
                        options.SubscriptionsTable,
                        isCentralized: true))
                    .Routing(r => r.TypeBased().MapAssemblyOf<CodeAddedIntegrationEvent>(options.InputQueue));
            },
            onCreated: async bus =>
            {
                await bus.Subscribe<CodeAddedIntegrationEvent>().ConfigureAwait(false);
            });

        services.AutoRegisterHandlersFromAssemblyOf<CodeAddedIntegrationEventHandler>();

        return services;
    }
}
