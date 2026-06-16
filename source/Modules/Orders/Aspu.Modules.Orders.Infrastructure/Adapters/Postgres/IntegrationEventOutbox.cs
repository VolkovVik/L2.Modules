using Aspu.Common.Application.Ports.MessageBus;
using Aspu.Modules.Orders.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Aspu.Modules.Orders.Infrastructure.Adapters.Postgres;

internal sealed class IntegrationEventOutbox(
    IOptions<OrdersMessagingOptions> options,
    ILogger<IntegrationEventOutbox> logger) : IIntegrationEventOutbox
{
    public async Task EnqueueAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var (typeName, payload) = IntegrationEventEnvelopeSerializer.Serialize(integrationEvent);

        await using var connection = new NpgsqlConnection(options.Value.ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = """
            INSERT INTO orders.outbox_messages (id, type_name, payload, occurred_on_utc)
            VALUES (@id, @typeName, @payload::jsonb, @occurredOnUtc)
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", integrationEvent.EventId);
        command.Parameters.AddWithValue("typeName", typeName);
        command.Parameters.AddWithValue("payload", payload);
        command.Parameters.AddWithValue("occurredOnUtc", integrationEvent.OccurredOnUtc);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Enqueued integration event {EventType} with id {EventId}", typeName, integrationEvent.EventId);
    }
}
