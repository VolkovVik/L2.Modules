using System.Threading.Channels;
using Aspu.Api.Options;
using Aspu.Common.Application.Ports.SignalrPort;
using Microsoft.Extensions.Options;

namespace Aspu.Api.Adapters.SignalR;

internal sealed class SignalrNotificationChannel : ISignalrNotificationChannel
{
    private readonly Channel<ISignalrNotification> _queue;

    public SignalrNotificationChannel(
        IOptions<SignalROptions> signalrOptions)
    {
        var options = signalrOptions.Value;
        var capacity = Math.Max(1, options.ChannelCapacity);

        _queue = Channel.CreateBounded<ISignalrNotification>(
            new BoundedChannelOptions(capacity)
            {
                SingleReader = options.SingleReader,
                SingleWriter = options.SingleWriter,
                FullMode = BoundedChannelFullMode.Wait,
            });
    }

    public ChannelReader<ISignalrNotification> Reader => _queue.Reader;

    public ValueTask WriteAsync(ISignalrNotification notification, CancellationToken cancellationToken = default) =>
        _queue.Writer.WriteAsync(notification, cancellationToken);
}
