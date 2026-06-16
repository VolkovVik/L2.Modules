using Aspu.Modules.Orders.IntegrationEvents.Codes;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Aspu.Modules.Orders.Infrastructure.Adapters.Rebus.Handlers;

internal sealed class CodeAddedIntegrationEventHandler(ILogger<CodeAddedIntegrationEventHandler> logger)
    : IHandleMessages<CodeAddedIntegrationEvent>
{
    public Task Handle(CodeAddedIntegrationEvent message)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Integration event received: code {CodeId} added for order {OrderId}",
                message.CodeId,
                message.OrderId);
        }

        return Task.CompletedTask;
    }
}
