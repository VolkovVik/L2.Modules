using System.Text.Json;
using Aspu.Common.Presentation.Abstractions.MqttAdapter;
using Aspu.Modules.Orders.Application.UseCases.Codes.Commands.AddCode;
using Mediator;

namespace Aspu.Modules.Orders.Presentation.Adapters.Mqtt;

internal sealed class MqttAddCodeHandler(IMediator mediator) : IMqttHandler
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

internal sealed class OrdersAddCodeHandler1(IMediator mediator) : IMqttHandler
{
    public string Topic => "/test/topic1";

    public async Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
        await mediator.Send(new AddCodeCommand(Guid.NewGuid(), Guid.NewGuid(), "test"), cancellationToken);
}

internal sealed class OrdersAddCodeHandler2(IMediator mediator) : IMqttHandler
{
    public string Topic => "/test/topic1";

    public async Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
        await mediator.Send(new AddCodeCommand(Guid.NewGuid(), Guid.NewGuid(), "test"), cancellationToken);
}
