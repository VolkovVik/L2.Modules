namespace Aspu.Api.Adapters.Mqtt;

internal sealed record MqttInboundMessage
{
    public required string Topic { get; init; }

    public required byte[] Payload { get; init; }
}
