namespace Aspu.Common.Presentation.Abstractions.MqttAdapter;

public interface IMqttHandler
{
    string Topic { get; }
    Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken);
}
