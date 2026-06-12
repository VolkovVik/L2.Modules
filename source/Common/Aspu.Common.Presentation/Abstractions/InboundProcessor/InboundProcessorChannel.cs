using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

/// <summary>
/// In-memory queue from MQTT receive path to the background processor (single reader).
/// Capacity from <see cref="TOptions.InboundProcessorQueueCapacity"/>;
/// when full, <see cref="BoundedChannelFullMode.DropWrite"/>.
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
