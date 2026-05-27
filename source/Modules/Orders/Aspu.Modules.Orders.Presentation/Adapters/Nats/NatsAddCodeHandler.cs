using System.Text.Json;
using Aspu.Common.Presentation.Abstractions.NatsAdapter;
using Aspu.Modules.Orders.Application.UseCases.Codes.Commands.AddCode;
using Mediator;

namespace Aspu.Modules.Orders.Presentation.Adapters.Nats;

internal sealed class NatsAddCodeHandler(IMediator mediator) : INatsHandler
{
    public string Topic => "/test/topic";

    public async Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        var command = JsonSerializer.Deserialize<AddCodeCommand>(payload.Span);
        if (command is null)
            return;

        await mediator.Send(command, cancellationToken);
    }
}

internal sealed class OrdersAddCodeHandler1(IMediator mediator) : INatsHandler
{
    public string Topic => "/test/topic1";

    public async Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
        await mediator.Send(new AddCodeCommand(Guid.NewGuid(), Guid.NewGuid(), "test"), cancellationToken);
}

internal sealed class OrdersAddCodeHandler2(IMediator mediator) : INatsHandler
{
    public string Topic => "/test/topic1";

    public async Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
        await mediator.Send(new AddCodeCommand(Guid.NewGuid(), Guid.NewGuid(), "test"), cancellationToken);
}
