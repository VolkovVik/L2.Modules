using System.Threading.Channels;
using Aspu.Api.Options;
using Microsoft.Extensions.Options;

namespace Aspu.Api.Adapters.Mqtt;

/// <summary>
/// In-memory queue from MQTT receive path to the background processor (single reader).
/// Capacity from <see cref="MqttOptions.InboundProcessorQueueCapacity"/>;
/// when full, <see cref="BoundedChannelFullMode.DropWrite"/>.
/// </summary>
internal sealed class MqttInboundMessageQueue(IOptions<MqttOptions> Options)
{
    private readonly Channel<MqttInboundMessage> _channel =
        Channel.CreateBounded<MqttInboundMessage>(
            new BoundedChannelOptions(Math.Max(1, Options.Value.InboundProcessorQueueCapacity))
            {
                SingleReader = true,
                SingleWriter = true,
                AllowSynchronousContinuations = false,
                FullMode = BoundedChannelFullMode.DropWrite,
            });

    public ChannelReader<MqttInboundMessage> Reader => _channel.Reader;

    public bool TryEnqueue(MqttInboundMessage message) => _channel.Writer.TryWrite(message);

    public void CompleteWriter(Exception? error = null) => _channel.Writer.TryComplete(error);
}
