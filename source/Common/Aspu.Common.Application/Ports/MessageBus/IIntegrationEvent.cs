namespace Aspu.Common.Application.Ports.MessageBus;

public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTime OccurredOnUtc { get; }
}
