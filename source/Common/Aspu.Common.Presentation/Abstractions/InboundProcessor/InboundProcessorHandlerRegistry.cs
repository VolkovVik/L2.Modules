using Microsoft.Extensions.DependencyInjection;

namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

/// <summary>
/// Snapshot of handler topics from DI, built once at startup (single <see cref="THandler"/> enumeration).
/// Shared by the subscriber (subscription list) and the inbound processor (fast reject of unknown topics).
/// </summary>
public sealed class InboundProcessorHandlerRegistry<THandler>
    where THandler : IInboundProcessorHandler
{
    private readonly HashSet<string> _topics;

    public InboundProcessorHandlerRegistry(IServiceScopeFactory scopeFactory)
    {
        using var scope = scopeFactory.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<THandler>();
        _topics = handlers
            .Select(h => h.Topic.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<string> GetSubscriptions() => [.. _topics];

    public bool IsEnabled(string topic) =>
        !string.IsNullOrWhiteSpace(topic) && _topics.Contains(topic.Trim());
}
