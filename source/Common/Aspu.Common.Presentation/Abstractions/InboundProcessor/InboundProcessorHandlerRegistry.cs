using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

/// <summary>
/// Dispatch table of handler topics from DI registrations (<see cref="InboundProcessorTopic{THandler}"/>), built once at startup without instantiating handlers.
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
    private readonly InboundProcessorTopic<THandler>[] _wildcardHandlers;
    private readonly ConcurrentDictionary<string, Type> _resolveCache = new(StringComparer.Ordinal);

    public InboundProcessorHandlerRegistry(
        IEnumerable<InboundProcessorTopic<THandler>> topics,
        IServiceProviderIsKeyedService keyedServices,
        ITopicMatcher matcher,
        ILogger<InboundProcessorHandlerRegistry<THandler>> logger)
    {
        _matcher = matcher;
        _logger = logger;

        var handlers = topics.ToList();
        CheckValidate(handlers);
        CheckDuplicate(handlers);
        CheckOverlap(handlers);
        CheckKeyed(handlers, keyedServices);

        // Handler type is the keyed-service key, so the processor resolves only the matching handler.
        _handlerTypes = handlers.ToFrozenDictionary(h => h.Pattern, h => h.HandlerType, StringComparer.Ordinal);

        _wildcardHandlers = [.. handlers.Where(h => matcher.IsWildcard(h.Pattern))];
    }

    public bool IsEmpty => _handlerTypes.Count == 0;

    public IReadOnlyList<string> GetSubscriptions() => [.. _handlerTypes.Keys];

    public bool TryResolve(string topic, [NotNullWhen(true)] out Type? handlerType)
    {
        handlerType = null;

        if (string.IsNullOrWhiteSpace(topic))
            return false;

        if (_handlerTypes.TryGetValue(topic, out handlerType))
            return true;

        if (_resolveCache.TryGetValue(topic, out handlerType))
            return true;

        handlerType = Array.Find(_wildcardHandlers, h => _matcher.IsMatch(h.Pattern, topic))?.HandlerType;
        if (handlerType is null)
            return false;

        if (Volatile.Read(ref _resolveCacheCount) < MaxCachedTopics && _resolveCache.TryAdd(topic, handlerType))
            Interlocked.Increment(ref _resolveCacheCount);

        return true;
    }

    private void CheckValidate(List<InboundProcessorTopic<THandler>> handlers)
    {
        var invalid = handlers
            .Where(h => !_matcher.IsValid(h.Pattern))
            .Take(MaxShowErrors)
            .ToList();
        if (!invalid.Any())
            return;

        if (_logger.IsEnabled(LogLevel.Error))
            InboundProcessorLog.InvalidTopics(_logger, _matcher.Name, string.Join(", ", invalid));

        throw new InvalidOperationException(
            $"Inbound processor handlers have invalid topic patterns for {_matcher.Name}");
    }

    private void CheckDuplicate(List<InboundProcessorTopic<THandler>> handlers)
    {
        var duplicates = handlers
            .GroupBy(h => h.Pattern, StringComparer.Ordinal)
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

    private void CheckOverlap(List<InboundProcessorTopic<THandler>> handlers)
    {
        var overlaps = new List<(InboundProcessorTopic<THandler> First, InboundProcessorTopic<THandler> Second)>();
        for (var i = 0; i < handlers.Count - 1 && overlaps.Count < MaxShowErrors; i++)
        {
            var first = handlers[i];
            for (var j = i + 1; j < handlers.Count && overlaps.Count < MaxShowErrors; j++)
            {
                var second = handlers[j];
                if (_matcher.IsMatch(first.Pattern, second.Pattern) || _matcher.IsMatch(second.Pattern, first.Pattern))
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

    private void CheckKeyed(List<InboundProcessorTopic<THandler>> handlers, IServiceProviderIsKeyedService keyedServices)
    {
        var notKeyed = handlers
            .Where(h => !keyedServices.IsKeyedService(typeof(THandler), h.HandlerType))
            .Take(MaxShowErrors)
            .ToList();
        if (!notKeyed.Any())
            return;

        if (_logger.IsEnabled(LogLevel.Error))
            InboundProcessorLog.NotKeyedHandlers(_logger, _matcher.Name, string.Join(", ", notKeyed));

        throw new InvalidOperationException(
            $"Inbound processor handlers aren't registered as keyed services for {_matcher.Name}");
    }
}
