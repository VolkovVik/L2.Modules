using System.Threading.Channels;
using Aspu.Api.Options;
using Microsoft.Extensions.Options;

namespace Aspu.Api.Adapters.SignalR;

internal sealed class SignalrMessageChannel
{
    private readonly Channel<SignalrMessage> _queue;

    public SignalrMessageChannel(IOptions<SignalROptions> signalrOptions)
    {
        var options = signalrOptions.Value;
        var capacity = Math.Max(1, options.ChannelCapacity);

        _queue = Channel.CreateBounded<SignalrMessage>(
            new BoundedChannelOptions(capacity)
            {
                SingleReader = options.SingleReader,
                SingleWriter = options.SingleWriter,
                FullMode = BoundedChannelFullMode.Wait,
            });
    }

    public ChannelReader<SignalrMessage> Reader => _queue.Reader;

    public ChannelWriter<SignalrMessage> Writer => _queue.Writer;
}
