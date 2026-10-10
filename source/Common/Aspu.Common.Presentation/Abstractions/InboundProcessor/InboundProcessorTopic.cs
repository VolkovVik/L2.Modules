namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

/// <summary>
/// Handler topic pattern known at registration time (the generator reads the handler's constant <see cref="IInboundProcessorHandler.Topic"/>),
/// so <see cref="InboundProcessorHandlerRegistry{THandler}"/> builds its table without instantiating handlers.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed", Justification = "Separates NATS and MQTT topic registrations in DI")]
public sealed record InboundProcessorTopic<THandler>(string Pattern, Type HandlerType)
    where THandler : IInboundProcessorHandler
{
    public override string ToString() => $"{HandlerType.Name} ('{Pattern}')";
}
