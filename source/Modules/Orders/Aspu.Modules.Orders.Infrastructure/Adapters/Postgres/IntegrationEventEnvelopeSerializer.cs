using System.Text.Json;
using Aspu.Common.Application.Ports.MessageBus;

namespace Aspu.Modules.Orders.Infrastructure.Adapters.Postgres;

internal static class IntegrationEventEnvelopeSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static (string TypeName, string Payload) Serialize(IIntegrationEvent integrationEvent)
    {
        var typeName = integrationEvent.GetType().AssemblyQualifiedName
            ?? throw new InvalidOperationException($"Cannot resolve type name for {integrationEvent.GetType().FullName}.");

        var payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), SerializerOptions);
        return (typeName, payload);
    }

    public static IIntegrationEvent Deserialize(string typeName, string payload)
    {
        var eventType = Type.GetType(typeName, throwOnError: true)
            ?? throw new InvalidOperationException($"Cannot resolve integration event type '{typeName}'.");

        if (!typeof(IIntegrationEvent).IsAssignableFrom(eventType))
            throw new InvalidOperationException($"Type '{typeName}' is not an integration event.");

        return (JsonSerializer.Deserialize(payload, eventType, SerializerOptions) as IIntegrationEvent)
            ?? throw new InvalidOperationException($"Failed to deserialize integration event '{typeName}'.");
    }
}
