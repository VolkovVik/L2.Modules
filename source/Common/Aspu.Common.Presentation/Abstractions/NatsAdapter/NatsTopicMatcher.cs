using Aspu.Common.Presentation.Abstractions.InboundProcessor;

namespace Aspu.Common.Presentation.Abstractions.NatsAdapter;

/// <summary>
/// NATS subject syntax: <c>.</c> separates tokens, <c>*</c> matches exactly one token,
/// <c>&gt;</c> matches one or more trailing tokens (last token only).
/// </summary>
public sealed class NatsTopicMatcher : ITopicMatcher
{
    private const char Separator = '.';

    public string Name => nameof(NatsTopicMatcher);

    public bool IsValid(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        var span = pattern.AsSpan();
        if (span.ContainsAny(" \t\r\n"))
            return false;

        var tokens = span.Split(Separator);
        var isTail = false;
        while (tokens.MoveNext())
        {
            var token = span[tokens.Current];
            if (token.IsEmpty || isTail)
                return false;

            isTail = token is ">";
        }

        return true;
    }

    public bool IsWildcard(string pattern)
    {
        var span = pattern.AsSpan();
        var tokens = span.Split(Separator);
        while (tokens.MoveNext())
        {
            if (span[tokens.Current] is "*" or ">")
                return true;
        }

        return false;
    }

    public bool IsMatch(string pattern, string topic)
    {
        var patternSpan = pattern.AsSpan();
        var topicSpan = topic.AsSpan();
        var patternTokens = patternSpan.Split(Separator);
        var topicTokens = topicSpan.Split(Separator);

        while (patternTokens.MoveNext())
        {
            var patternToken = patternSpan[patternTokens.Current];
            if (patternToken is ">")
                return topicTokens.MoveNext();

            if (!topicTokens.MoveNext())
                return false;

            if (patternToken is "*")
                continue;

            if (!patternToken.SequenceEqual(topicSpan[topicTokens.Current]))
                return false;
        }

        return !topicTokens.MoveNext();
    }
}
