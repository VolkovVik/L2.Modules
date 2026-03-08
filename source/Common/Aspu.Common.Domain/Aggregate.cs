using Aspu.Common.Domain.Events;
using CSharpFunctionalExtensions;

namespace Aspu.Common.Domain;

public abstract class Aggregate : Entity<Guid>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Aggregate(Guid id) : base(id) { }

    protected Aggregate() { }

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() =>
        [.. _domainEvents];

    public void ClearDomainEvents() =>
        _domainEvents.Clear();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);
}

public interface IAggregateRoot;
