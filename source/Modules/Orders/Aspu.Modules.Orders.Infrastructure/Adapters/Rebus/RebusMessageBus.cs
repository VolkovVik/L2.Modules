using Aspu.Common.Application.Ports.MessageBus;
using Rebus.Bus;

namespace Aspu.Modules.Orders.Infrastructure.Adapters.Rebus;

internal sealed class RebusMessageBus(IBus bus) : IMessageBus
{
    public Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default) =>
        bus.Publish(integrationEvent);
}
