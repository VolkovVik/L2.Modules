using System.Text.Json;
using Aspu.Common.Presentation.Abstractions.Mqtt;
using Aspu.Modules.Orders.Application.UseCases.Codes.Commands.AddCode;
using Mediator;

namespace Aspu.Modules.Orders.Presentation.Adapters.Mqtt;

public class MqttAddCodeHandler(IMediator mediator) : IMqttMessageHandler
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

public class OrdersAddCodeHandler1(IMediator mediator) : IMqttMessageHandler
{
    public string Topic => "/test/topic1";

    public async Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
        await mediator.Send(new AddCodeCommand(Guid.NewGuid(), Guid.NewGuid(), "test"), cancellationToken);
}

public class OrdersAddCodeHandler2(IMediator mediator) : IMqttMessageHandler
{
    public string Topic => "/test/topic1";

    public async Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
        await mediator.Send(new AddCodeCommand(Guid.NewGuid(), Guid.NewGuid(), "test"), cancellationToken);
}
