using Mediator;

namespace Aspu.Common.Domain.Events;

public interface IDomainEvent : INotification
{
    Guid Id { get; }

    DateTime OccurredOnUtc { get; }
}
