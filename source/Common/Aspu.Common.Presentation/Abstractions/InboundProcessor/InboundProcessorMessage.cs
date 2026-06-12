namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

public sealed record InboundProcessorMessage
{
    public required string Type { get; init; }

    public required string Topic { get; init; }

    public required byte[] Payload { get; init; }
}
