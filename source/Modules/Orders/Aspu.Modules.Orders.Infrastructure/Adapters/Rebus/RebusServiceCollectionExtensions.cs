using Aspu.Modules.Orders.Infrastructure.Adapters.Rebus.Handlers;
using Aspu.Modules.Orders.Infrastructure.Options;
using Aspu.Modules.Orders.IntegrationEvents.Codes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;
using Rebus.Transport.InMem;

namespace Aspu.Modules.Orders.Infrastructure.Adapters.Rebus;

internal static class RebusServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersRebus(this IServiceCollection services)
    {
        services.AddSingleton<InMemNetwork>();

        services.AddRebus(
            (configure, provider) =>
            {
                var options = provider.GetRequiredService<IOptions<OrdersMessagingOptions>>().Value;
                var network = provider.GetRequiredService<InMemNetwork>();

                return configure
                    .Transport(t => t.UseInMemoryTransport(network, options.InputQueue))
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
