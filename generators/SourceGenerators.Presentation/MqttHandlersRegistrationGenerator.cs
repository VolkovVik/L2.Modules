using Microsoft.CodeAnalysis;

namespace SourceGenerators.Presentation;

[Generator]
public sealed class MqttHandlersRegistrationGenerator()
    : BaseHandlersRegistrationGenerator(
        interfaceName: "IMqttHandler",
        interfaceNamespace: "Aspu.Common.Presentation.Abstractions.MqttAdapter",
        transport: "Mqtt");