namespace Aspu.Common.Application.Ports.MessageBus;

public interface IMessageBus
{
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
