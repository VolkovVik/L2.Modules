using Microsoft.CodeAnalysis;

namespace SourceGenerators.Presentation;

[Generator]
public sealed class NatsHandlersRegistrationGenerator()
    : BaseHandlersRegistrationGenerator(
        interfaceName: "INatsHandler",
        interfaceNamespace: "Aspu.Common.Presentation.Abstractions.NatsAdapter",
        transport: "Nats");