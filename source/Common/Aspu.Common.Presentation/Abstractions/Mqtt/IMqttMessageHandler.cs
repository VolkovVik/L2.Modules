namespace Aspu.Common.Presentation.Abstractions.Mqtt;

public interface IMqttMessageHandler
{
    string Topic { get; }
    Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken);
}
