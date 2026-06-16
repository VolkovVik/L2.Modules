namespace Aspu.Modules.Orders.Infrastructure.Options;

public sealed class OrdersMessagingOptions
{
    public const string SectionName = "Orders:Messaging";

    public bool Enabled { get; init; } = true;

    public string ConnectionString { get; init; } = string.Empty;

    public string InputQueue { get; init; } = "orders";

    public string TransportTable { get; init; } = "rebus_messages";

    public string SubscriptionsTable { get; init; } = "rebus_subscriptions";

    public int OutboxPollIntervalSeconds { get; init; } = 2;
}
