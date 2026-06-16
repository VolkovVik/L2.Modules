namespace Aspu.Modules.Orders.Infrastructure.Options;

public sealed class OrdersMessagingOptions
{
    public const string SectionName = "Orders:Messaging";

    public bool Enabled { get; init; } = true;

    public string InputQueue { get; init; } = "orders";

    public int OutboxChannelCapacity { get; init; } = 1000;
}
