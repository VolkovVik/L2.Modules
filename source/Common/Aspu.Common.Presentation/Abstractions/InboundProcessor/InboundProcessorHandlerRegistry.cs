using Microsoft.Extensions.DependencyInjection;

namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

/// <summary>
/// Snapshot of handler topics from DI, built once at startup (single <see cref="THandler"/> enumeration).
/// Shared by the subscriber (subscription list) and the inbound processor (fast reject of unknown topics).
/// </summary>
public sealed class InboundProcessorHandlerRegistry<THandler>(
    IServiceScopeFactory scopeFactory)
    where THandler : IInboundProcessorHandler
{
    private HashSet<string> _topics;

    public IReadOnlyList<string> GetSubscriptions() => [.. _topics];

    public bool IsEnabled(string topic)
    {
        if (_topics is null || _topics.Count < 1)
        {
            using var scope = scopeFactory.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<THandler>();
            _topics = handlers
                .Select(h => h.Topic.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        return !string.IsNullOrWhiteSpace(topic) && _topics.Contains(topic.Trim());
    }
}
