using Aspu.Common.Application.Ports.MessageBus;
using Aspu.Modules.Orders.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Aspu.Modules.Orders.Infrastructure.Adapters.Postgres;

internal sealed class OutboxDispatcherHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<OrdersMessagingOptions> options,
    ILogger<OutboxDispatcherHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
            return;

        var pollInterval = TimeSpan.FromSeconds(Math.Max(1, options.Value.OutboxPollIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var dispatched = await DispatchBatchAsync(stoppingToken).ConfigureAwait(false);
                if (dispatched == 0)
                    await Task.Delay(pollInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox dispatcher failed");
                await Task.Delay(pollInterval, stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task<int> DispatchBatchAsync(CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(options.Value.ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var pendingMessages = await ReadPendingMessagesAsync(connection, transaction, cancellationToken).ConfigureAwait(false);

        if (pendingMessages.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return 0;
        }

        await PublishMessagesAsync(pendingMessages, cancellationToken).ConfigureAwait(false);
        await MarkMessagesDispatchedAsync(connection, transaction, pendingMessages, cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Dispatched {Count} outbox messages", pendingMessages.Count);

        return pendingMessages.Count;
    }

    private static async Task<List<OutboxMessage>> ReadPendingMessagesAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string selectSql = """
            SELECT id, type_name, payload::text, occurred_on_utc
            FROM orders.outbox_messages
            WHERE dispatched_on_utc IS NULL
            ORDER BY created_on_utc
            LIMIT 50
            FOR UPDATE SKIP LOCKED
            """;

        var pendingMessages = new List<OutboxMessage>();

        await using var selectCommand = new NpgsqlCommand(selectSql, connection, transaction);
        await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            pendingMessages.Add(new OutboxMessage(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                await reader.GetFieldValueAsync<DateTime>(3, cancellationToken).ConfigureAwait(false)));
        }

        return pendingMessages;
    }

    private async Task PublishMessagesAsync(
        IReadOnlyList<OutboxMessage> pendingMessages,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        foreach (var message in pendingMessages)
        {
            var integrationEvent = IntegrationEventEnvelopeSerializer.Deserialize(message.TypeName, message.Payload);
            await messageBus.PublishAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task MarkMessagesDispatchedAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        IReadOnlyList<OutboxMessage> pendingMessages,
        CancellationToken cancellationToken)
    {
        const string updateSql = """
            UPDATE orders.outbox_messages
            SET dispatched_on_utc = @dispatchedOnUtc
            WHERE id = @id
            """;

        foreach (var message in pendingMessages)
        {
            await using var updateCommand = new NpgsqlCommand(updateSql, connection, transaction);
            updateCommand.Parameters.AddWithValue("id", message.Id);
            updateCommand.Parameters.AddWithValue("dispatchedOnUtc", DateTime.UtcNow);
            await updateCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed record OutboxMessage(Guid Id, string TypeName, string Payload, DateTime OccurredOnUtc);
}
