using Microsoft.CodeAnalysis;

namespace SourceGenerators.Presentation;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor NonConstantTopic = new(
        id: "ASPU001",
        title: "Inbound handler topic must be a compile-time constant string",
        messageFormat: "Handler '{0}' implements {1} but its Topic isn't a compile-time constant string",
        category: "Aspu.Presentation",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NotRegistrableHandler = new(
        id: "ASPU002",
        title: "Inbound handler can't be registered",
        messageFormat: "Handler '{0}' implements {1} but isn't registered: it must be sealed and internal or public (including containing types)",
        category: "Aspu.Presentation",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EmptyTopic = new(
        id: "ASPU003",
        title: "Inbound handler topic is empty",
        messageFormat: "Handler '{0}' implements {1} but its Topic is empty or whitespace",
        category: "Aspu.Presentation",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GenericHandler = new(
        id: "ASPU004",
        title: "Inbound handler can't be generic",
        messageFormat: "Handler '{0}' implements {1} but is generic or nested in a generic type, so it can't be registered",
        category: "Aspu.Presentation",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
