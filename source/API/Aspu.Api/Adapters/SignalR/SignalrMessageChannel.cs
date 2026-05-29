using System.Threading.Channels;
using Aspu.Api.Options;
using Microsoft.Extensions.Options;

namespace Aspu.Api.Adapters.SignalR;

internal sealed class SignalrMessageChannel
{
    private readonly Channel<SignalrMessageValue> _queue;

    public SignalrMessageChannel(IOptions<SignalROptions> signalrOptions)
    {
        var options = signalrOptions.Value;
        var capacity = Math.Max(1, options.ChannelCapacity);

        _queue = Channel.CreateBounded<SignalrMessageValue>(
            new BoundedChannelOptions(capacity)
            {
                SingleReader = options.SingleReader,
                SingleWriter = options.SingleWriter,
                FullMode = BoundedChannelFullMode.Wait,
            });
    }

    public ChannelReader<SignalrMessageValue> Reader => _queue.Reader;

    public ChannelWriter<SignalrMessageValue> Writer => _queue.Writer;
}
