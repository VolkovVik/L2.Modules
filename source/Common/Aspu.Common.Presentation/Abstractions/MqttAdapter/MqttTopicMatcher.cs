using Aspu.Common.Presentation.Abstractions.InboundProcessor;

namespace Aspu.Common.Presentation.Abstractions.MqttAdapter;

/// <summary>
/// MQTT topic filter syntax: <c>/</c> separates levels, <c>+</c> matches exactly one level (may be empty),
/// <c>#</c> matches zero or more trailing levels (last level only). Topics starting with <c>$</c>
/// are not matched by a leading wildcard.
/// </summary>
public sealed class MqttTopicMatcher : ITopicMatcher
{
    private const char Separator = '/';

    public string Name => nameof(MqttTopicMatcher);

    public bool IsValid(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        var span = pattern.AsSpan();
        var levels = span.Split(Separator);
        var isTail = false;
        while (levels.MoveNext())
        {
            var level = span[levels.Current];
            if (isTail)
                return false;

            if (level is "#")
            {
                isTail = true;
                continue;
            }

            if (level is not "+" && level.ContainsAny('+', '#'))
                return false;
        }

        return true;
    }

    public bool IsWildcard(string pattern) =>
        pattern.AsSpan().ContainsAny('+', '#');

    public bool IsMatch(string pattern, string topic)
    {
        if (topic.StartsWith('$') && pattern.Length > 0 && pattern[0] is '+' or '#')
            return false;

        var patternSpan = pattern.AsSpan();
        var topicSpan = topic.AsSpan();
        var patternLevels = patternSpan.Split(Separator);
        var topicLevels = topicSpan.Split(Separator);

        while (patternLevels.MoveNext())
        {
            var patternLevel = patternSpan[patternLevels.Current];
            if (patternLevel is "#")
                return true;

            if (!topicLevels.MoveNext())
                return false;

            if (patternLevel is "+")
                continue;

            if (!patternLevel.SequenceEqual(topicSpan[topicLevels.Current]))
                return false;
        }

        return !topicLevels.MoveNext();
    }
}
