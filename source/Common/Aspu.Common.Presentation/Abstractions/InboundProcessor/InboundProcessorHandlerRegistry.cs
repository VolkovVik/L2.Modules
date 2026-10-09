using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

/// <summary>
/// Dispatch table of handler topics from DI, built once at startup (single <see cref="THandler"/> enumeration).
/// Shared by the subscriber (subscription list) and the inbound processor (concrete topic → matching handler pattern).
/// Handler topics may contain transport wildcards; matching is delegated to <see cref="ITopicMatcher"/>.
/// </summary>
public sealed class InboundProcessorHandlerRegistry<THandler>
    where THandler : IInboundProcessorHandler
{
    private const int MaxShowErrors = 10;
    private const int MaxCachedTopics = 1_000;

    private readonly ITopicMatcher _matcher;
    private readonly ILogger<InboundProcessorHandlerRegistry<THandler>> _logger;

    private int _resolveCacheCount;
    private readonly FrozenDictionary<string, Type> _handlerTypes;
    private readonly FrozenSet<string> _wildcardSet;
    private readonly ConcurrentDictionary<string, string> _resolveCache = new(StringComparer.Ordinal);

    public InboundProcessorHandlerRegistry(
        IServiceScopeFactory scopeFactory,
        ITopicMatcher matcher,
        ILogger<InboundProcessorHandlerRegistry<THandler>> logger)
    {
        _matcher = matcher;
        _logger = logger;

        using var scope = scopeFactory.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<THandler>()
            .Select(h => new HandlerInfo(h.Topic.Trim(), h.GetType()))
            .ToList();

        CheckValidate(handlers);
        CheckDuplicate(handlers);
        CheckOverlap(handlers);

        // Handler type is the keyed-service key, so the processor resolves only the matching handler.
        _handlerTypes = handlers.ToFrozenDictionary(h => h.Topic, h => h.HandlerType, StringComparer.Ordinal);

        _wildcardSet = _handlerTypes.Keys
           .Where(matcher.IsWildcard)
           .ToFrozenSet(StringComparer.Ordinal);
    }

    public bool IsEmpty => _handlerTypes.Count == 0;

    public IReadOnlyList<string> GetSubscriptions() => [.. _handlerTypes.Keys];

    /// <summary>Handler type (keyed-service key) for a pattern returned by <see cref="TryResolve"/>.</summary>
    public Type GetHandlerType(string pattern) => _handlerTypes[pattern];

    public bool TryResolve(string topic, [NotNullWhen(true)] out string? pattern)
    {
        pattern = null;

        if (string.IsNullOrWhiteSpace(topic))
            return false;

        if (_handlerTypes.ContainsKey(topic))
        {
            pattern = topic;
            return true;
        }

        if (_resolveCache.TryGetValue(topic, out pattern))
            return true;

        pattern = _wildcardSet.FirstOrDefault(t => _matcher.IsMatch(t, topic));
        if (pattern is null)
            return false;

        if (Volatile.Read(ref _resolveCacheCount) < MaxCachedTopics && _resolveCache.TryAdd(topic, pattern))
            Interlocked.Increment(ref _resolveCacheCount);

        return true;
    }

    private void CheckValidate(List<HandlerInfo> handlers)
    {
        var invalid = handlers
            .Where(h => !_matcher.IsValid(h.Topic))
            .Take(MaxShowErrors)
            .ToList();
        if (!invalid.Any())
            return;

        if (_logger.IsEnabled(LogLevel.Error))
            InboundProcessorLog.InvalidTopics(_logger, _matcher.Name, string.Join(", ", invalid));

        throw new InvalidOperationException(
            $"Inbound processor handlers have invalid topic patterns for {_matcher.Name}");
    }

    private void CheckDuplicate(List<HandlerInfo> handlers)
    {
        var duplicates = handlers
            .GroupBy(h => h.Topic, StringComparer.Ordinal)
            .Where(g => g.Skip(1).Any())
            .SelectMany(g => g)
            .Take(MaxShowErrors)
            .ToList();
        if (!duplicates.Any())
            return;

        if (_logger.IsEnabled(LogLevel.Error))
            InboundProcessorLog.DuplicateTopics(_logger, _matcher.Name, string.Join(", ", duplicates));

        throw new InvalidOperationException(
            $"Inbound processor handlers have duplicate topic patterns for {_matcher.Name}");
    }

    private void CheckOverlap(List<HandlerInfo> handlers)
    {
        var overlaps = new List<(HandlerInfo First, HandlerInfo Second)>();
        for (var i = 0; i < handlers.Count - 1 && overlaps.Count < MaxShowErrors; i++)
        {
            var first = handlers[i];
            for (var j = i + 1; j < handlers.Count && overlaps.Count < MaxShowErrors; j++)
            {
                var second = handlers[j];
                if (_matcher.IsMatch(first.Topic, second.Topic) || _matcher.IsMatch(second.Topic, first.Topic))
                {
                    overlaps.Add((First: first, Second: second));
                }
            }
        }
        if (!overlaps.Any())
            return;

        if (_logger.IsEnabled(LogLevel.Error))
            InboundProcessorLog.OverlappingTopics(_logger, _matcher.Name, string.Join(", ", overlaps.Select(o => $"{o.First} & {o.Second}")));

        throw new InvalidOperationException(
            $"Inbound processor handlers have overlapping topic patterns for {_matcher.Name}");
    }

    private sealed record HandlerInfo(string Topic, Type HandlerType)
    {
        public override string ToString() => $"{HandlerType.Name} ('{Topic}')";
    }
}
