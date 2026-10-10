namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

/// <summary>
/// Inbound message handler. <see cref="Topic"/> must be a compile-time constant (e.g. <c>public string Topic => "orders.*";</c>):
/// the registration source generator reads it, so handlers aren't instantiated to learn their topics.
/// </summary>
public interface IInboundProcessorHandler
{
    string Topic { get; }
    Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken);
}
