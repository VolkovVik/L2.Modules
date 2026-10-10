namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

/// <summary>
/// Inbound message handler; its topic pattern comes from <see cref="IInboundTopic"/>, which every handler implements too.
/// </summary>
public interface IInboundProcessorHandler
{
    Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken);
}
