using Aspu.Common.Application.Ports.MessageBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aspu.Modules.Orders.Infrastructure.Adapters.InMemory;

internal sealed class InMemoryOutboxDispatcherHostedService(
    InMemoryOutboxChannel channel,
    IServiceScopeFactory scopeFactory,
    ILogger<InMemoryOutboxDispatcherHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var integrationEvent in channel.Reader.ReadAllAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
                await messageBus.PublishAsync(integrationEvent, stoppingToken).ConfigureAwait(false);

                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug(
                        "Dispatched integration event {EventType} with id {EventId}",
                        integrationEvent.GetType().Name,
                        integrationEvent.EventId);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Failed to dispatch integration event {EventType}", integrationEvent.GetType().Name);
            }
        }
    }
}
