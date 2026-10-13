using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

/// <summary>
/// In-memory queue from a transport subscriber (NATS or MQTT, one channel per <typeparamref name="TOptions"/>)
/// to <see cref="InboundProcessorHostedService{TOptions, THandler}"/> (single writer, single reader).
/// Capacity from <see cref="IInboundProcessorOptions.InboundProcessorChannelCapacity"/>;
/// when full, <see cref="BoundedChannelFullMode.DropWrite"/> and <see cref="TryEnqueue"/> returns false.
/// </summary>
public sealed class InboundProcessorChannel<TOptions>(
    IOptions<TOptions> options)
    where TOptions : class, IInboundProcessorOptions
{
    private readonly Channel<InboundProcessorMessage> _channel =
        Channel.CreateBounded<InboundProcessorMessage>(
            new BoundedChannelOptions(Math.Max(1, options.Value.InboundProcessorChannelCapacity))
            {
                SingleReader = true,
                SingleWriter = true,
                AllowSynchronousContinuations = false,
                FullMode = BoundedChannelFullMode.DropWrite,
            });

    public ChannelReader<InboundProcessorMessage> Reader => _channel.Reader;

    public bool TryEnqueue(InboundProcessorMessage message) => _channel.Writer.TryWrite(message);

    public void CompleteWriter(Exception? error = null) => _channel.Writer.TryComplete(error);
}
