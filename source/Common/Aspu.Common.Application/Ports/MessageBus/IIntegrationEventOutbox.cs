namespace Aspu.Common.Application.Ports.MessageBus;

public interface IIntegrationEventOutbox
{
    Task EnqueueAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
