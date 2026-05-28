using Aspu.Common.Presentation.Abstractions.NatsAdapter;
using Aspu.Modules.Orders.Application.UseCases.Codes.Commands.AddCode;
using Mediator;

namespace Aspu.Modules.Orders.Presentation.Adapters.Nats;

internal sealed class NatsAddCodeHandler(IMediator mediator) : INatsHandler
{
    public string Topic => "test.message";

    public async Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
        await mediator.Send(new AddCodeCommand(Guid.NewGuid(), Guid.NewGuid(), "test"), cancellationToken);

}

internal sealed class OrdersAddCodeHandler1(IMediator mediator) : INatsHandler
{
    public string Topic => "sensors.soil.moisture1.*";

    public async Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
        await mediator.Send(new AddCodeCommand(Guid.NewGuid(), Guid.NewGuid(), "test"), cancellationToken);
}

internal sealed class OrdersAddCodeHandler2(IMediator mediator) : INatsHandler
{
    public string Topic => "sensors.soil.moisture2.*";

    public async Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
        await mediator.Send(new AddCodeCommand(Guid.NewGuid(), Guid.NewGuid(), "test"), cancellationToken);
}
