using Aspu.Common.Presentation.Abstractions.MqttAdapter;

namespace Aspu.Api.IntegrationTests.InboundProcessor;

internal sealed class MqttTopicMatcherShould
{
    private readonly MqttTopicMatcher _matcher = new();

    [Test]
    [Arguments("a/b", "a/b", true)]
    [Arguments("a/b", "a/B", false)]
    [Arguments("/test/topic", "/test/topic", true)]
    [Arguments("a/+", "a/b", true)]
    [Arguments("a/+", "a/", true)]
    [Arguments("a/+", "a/b/c", false)]
    [Arguments("a/+", "a", false)]
    [Arguments("a/+/c", "a/b/c", true)]
    [Arguments("a/#", "a", true)]
    [Arguments("a/#", "a/b", true)]
    [Arguments("a/#", "a/b/c", true)]
    [Arguments("a/#", "b/c", false)]
    [Arguments("#", "a/b", true)]
    [Arguments("+/x", "$SYS/x", false)]
    [Arguments("#", "$SYS/x", false)]
    [Arguments("$SYS/#", "$SYS/x", true)]
    public async Task Match_Topic(string pattern, string topic, bool expected) =>
        await Assert.That(_matcher.IsMatch(pattern, topic)).IsEqualTo(expected);

    [Test]
    [Arguments("a/b", false)]
    [Arguments("a/+", true)]
    [Arguments("a/#", true)]
    public async Task Detect_Wildcard(string pattern, bool expected) =>
        await Assert.That(_matcher.IsWildcard(pattern)).IsEqualTo(expected);

    [Test]
    [Arguments("a/b", true)]
    [Arguments("/a/b", true)]
    [Arguments("+/+/#", true)]
    [Arguments("#", true)]
    [Arguments("", false)]
    [Arguments("a/#/b", false)]
    [Arguments("a/b#", false)]
    [Arguments("a/b+", false)]
    public async Task Validate_Pattern(string pattern, bool expected) =>
        await Assert.That(_matcher.IsValid(pattern)).IsEqualTo(expected);
}
