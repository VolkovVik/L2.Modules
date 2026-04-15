using System.Text.Json;
using Aspu.Common.Presentation.Abstractions.Mqtt;
using Aspu.Modules.Orders.Application.UseCases.Codes.Commands.AddCode;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Aspu.Modules.Orders.Presentation.Adapters.Mqtt;

public class MqttAddCodeHandler(
    IMediator mediator,
    ILogger<MqttAddCodeHandler> logger) : IMqttMessageHandler
{
    public string Topic => "/test/topic";

    public async Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Mqtt publication on {@Topic}", topic);
        }
        var command = JsonSerializer.Deserialize<AddCodeCommand>(payload.Span);
        if (command is null)
            return;

        await mediator.Send(command, cancellationToken);
    }
}


public class OrdersAddCodeHandler1(
    IMediator mediator,
    ILogger<OrdersAddCodeHandler1> logger) : IMqttMessageHandler
{
    public string Topic => "/test/topic1";

    public async Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Mqtt publication on {@Topic}", topic);
        }

        await mediator.Send(new AddCodeCommand(Guid.NewGuid(), Guid.NewGuid(), "test"), cancellationToken);
    }
}

public class OrdersAddCodeHandler2(
    IMediator mediator,
    ILogger<OrdersAddCodeHandler2> logger) : IMqttMessageHandler
{
    public string Topic => "/test/topic1";

    public async Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Mqtt publication on {@Topic}", topic);
        }

        await mediator.Send(new AddCodeCommand(Guid.NewGuid(), Guid.NewGuid(), "test"), cancellationToken);
    }
}
