namespace Aspu.Common.Presentation.Abstractions.NatsAdapter;

public interface INatsHandler
{
    string Topic { get; }
    Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken);
}
