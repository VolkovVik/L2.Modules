using System.Threading.Channels;
using Aspu.Common.Application.Ports.MessageBus;
using Aspu.Modules.Orders.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Aspu.Modules.Orders.Infrastructure.Adapters.InMemory;

internal sealed class InMemoryOutboxChannel(IOptions<OrdersMessagingOptions> options)
{
    private readonly Channel<IIntegrationEvent> _channel =
        Channel.CreateBounded<IIntegrationEvent>(
            new BoundedChannelOptions(Math.Max(1, options.Value.OutboxChannelCapacity))
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
                FullMode = BoundedChannelFullMode.Wait,
            });

    public ChannelWriter<IIntegrationEvent> Writer => _channel.Writer;

    public ChannelReader<IIntegrationEvent> Reader => _channel.Reader;
}
