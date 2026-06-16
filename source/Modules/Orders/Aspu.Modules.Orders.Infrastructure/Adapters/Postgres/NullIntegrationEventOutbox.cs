using Aspu.Common.Application.Ports.MessageBus;

namespace Aspu.Modules.Orders.Infrastructure.Adapters.Postgres;

internal sealed class NullIntegrationEventOutbox : IIntegrationEventOutbox
{
    public Task EnqueueAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
