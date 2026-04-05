namespace Aspu.Common.Application.Ports.Messaging;

public interface IMqttMessageHandler
{
    Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken);
}
