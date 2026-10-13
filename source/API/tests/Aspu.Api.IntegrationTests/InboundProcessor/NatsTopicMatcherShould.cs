using Aspu.Common.Presentation.Abstractions.NatsAdapter;

namespace Aspu.Api.IntegrationTests.InboundProcessor;

internal sealed class NatsTopicMatcherShould
{
    private readonly NatsTopicMatcher _matcher = new();

    [Test]
    [Arguments("a.b", "a.b", true)]
    [Arguments("a.b", "a.c", false)]
    [Arguments("a.b", "A.b", false)]
    [Arguments("a.*", "a.b", true)]
    [Arguments("a.*", "a.b.c", false)]
    [Arguments("a.*", "a", false)]
    [Arguments("a.*.c", "a.b.c", true)]
    [Arguments("a.>", "a.b", true)]
    [Arguments("a.>", "a.b.c", true)]
    [Arguments("a.>", "a", false)]
    [Arguments(">", "a.b", true)]
    [Arguments("sensors.soil.moisture1.*", "sensors.soil.moisture1.x", true)]
    public async Task Match_Topic(string pattern, string topic, bool expected) =>
        await Assert.That(_matcher.IsMatch(pattern, topic)).IsEqualTo(expected);

    [Test]
    [Arguments("a.b", false)]
    [Arguments("a.*", true)]
    [Arguments("a.>", true)]
    [Arguments("a*.b", false)]
    public async Task Detect_Wildcard(string pattern, bool expected) =>
        await Assert.That(_matcher.IsWildcard(pattern)).IsEqualTo(expected);

    [Test]
    [Arguments("a.b", true)]
    [Arguments("a.*.>", true)]
    [Arguments("", false)]
    [Arguments("a..b", false)]
    [Arguments(".a", false)]
    [Arguments("a.>.b", false)]
    [Arguments("a b", false)]
    public async Task Validate_Pattern(string pattern, bool expected) =>
        await Assert.That(_matcher.IsValid(pattern)).IsEqualTo(expected);
}
