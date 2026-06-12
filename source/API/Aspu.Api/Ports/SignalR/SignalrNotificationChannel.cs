using System.Threading.Channels;
using Aspu.Api.Options;
using Aspu.Common.Application.Ports.SignalrPort;
using Microsoft.Extensions.Options;

namespace Aspu.Api.Ports.Signalr;

internal sealed class SignalrNotificationChannel : ISignalrNotificationChannel
{
    private readonly Channel<ISignalrNotification> _queue;

    public SignalrNotificationChannel(
        IOptions<SignalrOptions> signalrOptions)
    {
        var options = signalrOptions.Value;
        var capacity = Math.Max(1, options.ChannelCapacity);

        _queue = Channel.CreateBounded<ISignalrNotification>(
            new BoundedChannelOptions(capacity)
            {
                FullMode = options.FullMode,
                SingleReader = options.SingleReader,
                SingleWriter = options.SingleWriter,
                AllowSynchronousContinuations = options.AllowSynchronousContinuations,
            });
    }

    public ChannelReader<ISignalrNotification> Reader => _queue.Reader;

    public bool TryWrite(ISignalrNotification notification) =>
        _queue.Writer.TryWrite(notification);

    public ValueTask WriteAsync(ISignalrNotification notification, CancellationToken cancellationToken = default) =>
        _queue.Writer.WriteAsync(notification, cancellationToken);

    public bool CompleteWriter(Exception? error = null) =>
        _queue.Writer.TryComplete(error);
}
