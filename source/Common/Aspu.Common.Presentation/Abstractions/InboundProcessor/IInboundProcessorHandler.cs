namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

public interface IInboundProcessorHandler
{
    string Topic { get; }
    Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken);
}
