namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

public interface IInboundProcessorOptions
{
    bool Enabled { get; init; }

    /// <summary>
    /// Max concurrent inbound message handlers (each message still gets its own DI scope).
    /// Minimum effective value is 1.
    /// </summary>
    int InboundProcessorMaxDegreeOfParallelism { get; init; }

    /// <summary>
    /// Bounded channel capacity between receive and the inbound processor.
    /// Minimum effective value is 1.
    /// </summary>
    int InboundProcessorChannelCapacity { get; init; }
}
