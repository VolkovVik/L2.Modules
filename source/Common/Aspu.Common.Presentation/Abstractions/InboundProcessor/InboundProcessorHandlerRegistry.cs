using System.Collections.Concurrent;
using System.Collections.Frozen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

/// <summary>
/// Dispatch table of handler topics from DI, built once at startup (single <see cref="THandler"/> enumeration).
/// Shared by the subscriber (subscription list) and the inbound processor (concrete topic → matching handler patterns).
/// Handler topics may contain transport wildcards; matching is delegated to <see cref="ITopicMatcher"/>.
/// </summary>
public sealed class InboundProcessorHandlerRegistry<THandler>
    where THandler : IInboundProcessorHandler
{
    // Concrete subjects may be unbounded (device ids), so the topic resolution cache stops growing at this size.
    private const int MaxCachedTopics = 1_000;

    private readonly ITopicMatcher _matcher;
    private readonly FrozenSet<string> _topicSet;
    private readonly FrozenSet<string> _wildcardSet;
    private readonly ConcurrentDictionary<string, string[]> _resolveCache = new(-1, MaxCachedTopics, StringComparer.Ordinal);
    private int _resolveCacheCount;

    public InboundProcessorHandlerRegistry(
        IServiceScopeFactory scopeFactory,
        ITopicMatcher matcher,
        ILogger<InboundProcessorHandlerRegistry<THandler>> logger)
    {
        _matcher = matcher;

        using var scope = scopeFactory.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<THandler>()
            .Select(h => (Topic: h.Topic.Trim(), HandlerName: h.GetType().Name))
            .Where(h => !string.IsNullOrWhiteSpace(h.Topic))
            .ToList();

        foreach (var (topic, handlerName) in handlers.Where(h => !matcher.IsValid(h.Topic)))
        {
            if (logger.IsEnabled(LogLevel.Warning))
                logger.LogWarning("Inbound processor handler {Handler} has invalid topic pattern {HandlerTopic} for {Matcher} and is skipped", handlerName, topic, matcher.GetType().Name);
        }

        var topics = handlers
            .Select(h => h.Topic)
            .Where(matcher.IsValid)
            .ToList();

        _topicSet = topics.ToFrozenSet(StringComparer.Ordinal);

        _wildcardSet = topics
           .Where(matcher.IsWildcard)
           .ToFrozenSet(StringComparer.Ordinal);
    }

    public bool IsEmpty => _topicSet.Count == 0;

    public IReadOnlyList<string> GetSubscriptions() => [.. _topicSet];

    public bool TryResolve(string topic, out IReadOnlyList<string> patterns)
    {
        patterns = [];

        if (string.IsNullOrWhiteSpace(topic))
            return false;

        if (_resolveCache.TryGetValue(topic, out var items))
        {
            patterns = items;
            return true;
        }

        var list = new List<string>();
        if (_topicSet.Contains(topic))
            list.Add(topic);

        list.AddRange(_wildcardSet.Where(t => _matcher.IsMatch(t, topic)));
        if (!list.Any())
            return false;

        list = [.. list.Distinct(StringComparer.Ordinal)];
        if (Volatile.Read(ref _resolveCacheCount) < MaxCachedTopics && _resolveCache.TryAdd(topic, [.. list]))
            Interlocked.Increment(ref _resolveCacheCount);

        patterns = [.. list];
        return true;
    }
}
