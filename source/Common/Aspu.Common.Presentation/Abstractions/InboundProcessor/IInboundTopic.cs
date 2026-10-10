namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

/// <summary>
/// Static topic pattern (transport wildcards allowed) of an inbound handler, read at registration without instantiating it.
/// Kept apart from <see cref="IInboundProcessorHandler"/>: an interface with static abstract members can't be a generic type argument.
/// </summary>
public interface IInboundTopic
{
    static abstract string Topic { get; }
}
