namespace Aspu.Common.Domain.Events;

public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent(DateTime occurredOnUtc)
    {
        Id = Guid.NewGuid();
        OccurredOnUtc = occurredOnUtc;
    }

    protected DomainEvent(Guid id, DateTime occurredOnUtc)
    {
        Id = id;
        OccurredOnUtc = occurredOnUtc;
    }

    public Guid Id { get; init; }

    public DateTime OccurredOnUtc { get; init; }
}

