using Mediator;
using Serilog;

namespace Aspu.Modules.Orders.Domain.Model.CodeAggregate.DomainEvents;

public sealed record CodeValidatedDomainEvent(Guid CodeId, Guid OrderId, Guid OrderUnitId, string Value) : INotification;

public sealed class PingNotificationHandler : INotificationHandler<CodeValidatedDomainEvent>
{
    public ValueTask Handle(CodeValidatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log.Debug("Ping notification: {@Id}", notification.CodeId);
        return ValueTask.CompletedTask;
    }
}

///public sealed record CodeValidatedDomainEvent(Guid CodeId, Guid OrderId, Guid OrderUnitId, string Value) : DomainEvent;
///public sealed record CodeDefectedDomainEvent(Guid CodeId, Guid OrderId, Guid OrderUnitId, string Value) : DomainEvent;
///public sealed record CodeDefectedDomainEvent(Guid CodeId, Guid OrderId, Guid OrderUnitId, string Value) : DomainEvent;
///public sealed record CodeDefectedDomainEvent(Guid CodeId, Guid OrderId, Guid OrderUnitId, string Value) : DomainEvent;
