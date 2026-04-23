using Aspu.Common.Presentation.Abstractions.MqttAdapter;

namespace Aspu.Api.Adapters.Mqtt;

/// <summary>
/// Snapshot of MQTT handler topics from DI, built once at startup (single <see cref="IMqttHandler"/> enumeration).
/// Shared by the subscriber (subscription list) and the inbound processor (fast reject of unknown topics).
/// </summary>
internal sealed class MqttHandlerTopicRegistry
{
    private readonly HashSet<string> _topics;

    public MqttHandlerTopicRegistry(IServiceScopeFactory scopeFactory)
    {
        using var scope = scopeFactory.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IMqttHandler>();
        _topics = handlers
            .Select(h => h.Topic.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<string> SubscriptionTopics() => [.. _topics];

    public bool IsTopicEnabled(string topic) =>
        !string.IsNullOrWhiteSpace(topic) && _topics.Contains(topic.Trim());
}
