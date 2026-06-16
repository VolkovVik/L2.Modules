using Aspu.Common.Application.Ports.MessageBus;

namespace Aspu.Modules.Orders.Infrastructure.Adapters.InMemory;

internal sealed class InMemoryIntegrationEventOutbox(InMemoryOutboxChannel channel) : IIntegrationEventOutbox
{
    public async Task EnqueueAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default) =>
        await channel.Writer.WriteAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
}
