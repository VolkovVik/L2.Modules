using System.Threading.Channels;

namespace Aspu.Api.Adapters.SignaR;

internal class SignalrMessageChannel
{
    private const int Capacity = 10_000;

    private readonly Channel<SignalrMessageValue> _queue =
        Channel.CreateBounded<SignalrMessageValue>(new BoundedChannelOptions(Capacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait,
        });

    public ChannelReader<SignalrMessageValue> Reader => _queue.Reader;
    public ChannelWriter<SignalrMessageValue> Writer => _queue.Writer;
}
