using Aspu.Common.Application.Ports.MessageBus;

namespace Aspu.Modules.Orders.IntegrationEvents.Codes;

public sealed record CodeAddedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOnUtc,
    Guid CodeId,
    Guid OrderId,
    Guid OrderUnitId,
    string Value) : IIntegrationEvent
{
    public static CodeAddedIntegrationEvent Create(Guid codeId, Guid orderId, Guid orderUnitId, string value) =>
        new(Guid.CreateVersion7(), DateTime.UtcNow, codeId, orderId, orderUnitId, value);
}
