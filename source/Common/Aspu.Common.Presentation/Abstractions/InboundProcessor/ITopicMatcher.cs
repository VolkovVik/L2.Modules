namespace Aspu.Common.Presentation.Abstractions.InboundProcessor;

/// <summary>
/// Transport-specific topic/subject pattern syntax (wildcards, separators).
/// Matching is case-sensitive (ordinal), as in NATS and MQTT.
/// </summary>
public interface ITopicMatcher
{
    /// <summary>Returns true when <paramref name="pattern"/> is a syntactically valid subscription pattern.</summary>
    bool IsValid(string pattern);

    /// <summary>Returns true when <paramref name="pattern"/> contains wildcards (needs <see cref="IsMatch"/> instead of equality).</summary>
    bool IsWildcard(string pattern);

    /// <summary>Returns true when the concrete <paramref name="topic"/> matches the subscription <paramref name="pattern"/>.</summary>
    bool IsMatch(string pattern, string topic);
}
