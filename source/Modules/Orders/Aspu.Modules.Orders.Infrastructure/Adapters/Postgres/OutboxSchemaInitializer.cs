using Aspu.Modules.Orders.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Aspu.Modules.Orders.Infrastructure.Adapters.Postgres;

internal sealed class OutboxSchemaInitializer(
    IOptions<OrdersMessagingOptions> options,
    ILogger<OutboxSchemaInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
            return;

        await using var connection = new NpgsqlConnection(options.Value.ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = """
            CREATE SCHEMA IF NOT EXISTS orders;

            CREATE TABLE IF NOT EXISTS orders.outbox_messages
            (
                id uuid PRIMARY KEY,
                type_name text NOT NULL,
                payload jsonb NOT NULL,
                occurred_on_utc timestamptz NOT NULL,
                created_on_utc timestamptz NOT NULL DEFAULT now(),
                dispatched_on_utc timestamptz NULL
            );

            CREATE INDEX IF NOT EXISTS ix_outbox_messages_pending
                ON orders.outbox_messages (created_on_utc)
                WHERE dispatched_on_utc IS NULL;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Orders outbox schema initialized");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
