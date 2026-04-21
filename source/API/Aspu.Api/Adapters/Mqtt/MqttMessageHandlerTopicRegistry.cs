using Aspu.Common.Presentation.Abstractions.Mqtt;

namespace Aspu.Api.Adapters.Mqtt;

/// <summary>
/// Snapshot of MQTT handler topics from DI, built once at startup (single <see cref="IMqttMessageHandler"/> enumeration).
/// Shared by the subscriber (subscription list) and the inbound processor (fast reject of unknown topics).
/// </summary>
internal sealed class MqttMessageHandlerTopicRegistry
{
    private readonly HashSet<string> _topics;

    public MqttMessageHandlerTopicRegistry(IServiceScopeFactory scopeFactory)
    {
        using var scope = scopeFactory.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IMqttMessageHandler>();
        _topics = handlers
            .Select(h => h.Topic.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<string> SubscriptionTopics() => [.. _topics];

    public bool IsTopicEnabled(string topic) =>
        !string.IsNullOrWhiteSpace(topic) && _topics.Contains(topic.Trim());
}
