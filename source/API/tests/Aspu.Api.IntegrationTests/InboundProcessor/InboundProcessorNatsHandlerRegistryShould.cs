using Aspu.Common.Presentation.Abstractions.InboundProcessor;
using Aspu.Common.Presentation.Abstractions.NatsAdapter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Aspu.Api.IntegrationTests.InboundProcessor;

internal sealed class InboundProcessorNatsHandlerRegistryShould
{
    [Test]
    public async Task Resolve_Exact_Topic()
    {
        var registry = CreateRegistry(services => services
            .AddSingleton<INatsHandler, ExactHandler>());

        var isEnabled = registry.TryResolve("orders.created", out var patterns);

        await Assert.That(isEnabled).IsTrue();
        await Assert.That(patterns).IsEquivalentTo(["orders.created"]);
    }

    [Test]
    public async Task Resolve_Wildcard_Topic()
    {
        var registry = CreateRegistry(services => services
            .AddSingleton<INatsHandler, SingleTokenHandler>());

        var isEnabled = registry.TryResolve("orders.updated", out var patterns);

        await Assert.That(isEnabled).IsTrue();
        await Assert.That(patterns).IsEquivalentTo(["orders.*"]);
    }

    [Test]
    public async Task Resolve_Tail_Topic()
    {
        var registry = CreateRegistry(services => services
            .AddSingleton<INatsHandler, TailHandler>());

        var isEnabled = registry.TryResolve("orders.deleted.v2", out var patterns);

        await Assert.That(isEnabled).IsTrue();
        await Assert.That(patterns).IsEquivalentTo(["orders.>"]);
    }

    [Test]
    public async Task Resolve_All_Matching_Wildcards()
    {
        var registry = CreateRegistry(services => services
            .AddSingleton<INatsHandler, SingleTokenHandler>()
            .AddSingleton<INatsHandler, TailHandler>());

        var isEnabled = registry.TryResolve("orders.updated", out var patterns);

        await Assert.That(isEnabled).IsTrue();
        await Assert.That(patterns).IsEquivalentTo(["orders.*", "orders.>"]);
    }

    [Test]
    public async Task Resolve_Exact_And_Wildcard_Together()
    {
        var registry = CreateRegistry(services => services
            .AddSingleton<INatsHandler, ExactHandler>()
            .AddSingleton<INatsHandler, SingleTokenHandler>()
            .AddSingleton<INatsHandler, TailHandler>());

        var isEnabled = registry.TryResolve("orders.created", out var patterns);

        await Assert.That(isEnabled).IsTrue();
        await Assert.That(patterns).IsEquivalentTo(["orders.created", "orders.*", "orders.>"]);
    }

    [Test]
    public async Task Resolve_Cached_Wildcard_Topic()
    {
        var registry = CreateRegistry(services => services
            .AddSingleton<INatsHandler, SingleTokenHandler>());

        registry.TryResolve("orders.updated", out _);
        var isEnabled = registry.TryResolve("orders.updated", out var patterns);

        await Assert.That(isEnabled).IsTrue();
        await Assert.That(patterns).IsEquivalentTo(["orders.*"]);
    }

    [Test]
    public async Task Not_Resolve_Unknown_Topic()
    {
        var registry = CreateRegistry(services => services
            .AddSingleton<INatsHandler, ExactHandler>()
            .AddSingleton<INatsHandler, SingleTokenHandler>());

        var isEnabled = registry.TryResolve("payments.updated", out var patterns);

        await Assert.That(isEnabled).IsFalse();
        await Assert.That(patterns).IsEmpty();
    }

    [Test]
    public async Task Skip_Invalid_Topic()
    {
        var registry = CreateRegistry(services => services
            .AddSingleton<INatsHandler, ExactHandler>()
            .AddSingleton<INatsHandler, InvalidHandler>());

        var subscriptions = registry.GetSubscriptions();
        var isEnabled = registry.TryResolve("orders.x.created", out _);

        await Assert.That(subscriptions).IsEquivalentTo(["orders.created"]);
        await Assert.That(isEnabled).IsFalse();
    }

    [Test]
    public async Task Log_Warning_For_Invalid_Topic()
    {
        var logger = new CapturingLogger();
        CreateRegistry(services => services
            .AddSingleton<INatsHandler, ExactHandler>()
            .AddSingleton<INatsHandler, InvalidHandler>(), logger);

        await Assert.That(logger.Entries).Count().IsEqualTo(1);
        await Assert.That(logger.Entries[0].Level).IsEqualTo(LogLevel.Warning);
        await Assert.That(logger.Entries[0].Message).Contains(nameof(InvalidHandler));
        await Assert.That(logger.Entries[0].Message).Contains("orders.>.created");
    }

    [Test]
    public async Task Be_Empty_When_Only_Invalid_Topics()
    {
        var registry = CreateRegistry(services => services
            .AddSingleton<INatsHandler, InvalidHandler>());

        await Assert.That(registry.IsEmpty).IsTrue();
        await Assert.That(registry.GetSubscriptions()).IsEmpty();
    }

    [Test]
    public async Task Return_Patterns_As_Subscriptions()
    {
        var registry = CreateRegistry(services => services
            .AddSingleton<INatsHandler, ExactHandler>()
            .AddSingleton<INatsHandler, SingleTokenHandler>()
            .AddSingleton<INatsHandler, TailHandler>());

        var subscriptions = registry.GetSubscriptions();

        await Assert.That(subscriptions).IsEquivalentTo(["orders.created", "orders.*", "orders.>"]);
    }

    private static InboundProcessorHandlerRegistry<INatsHandler> CreateRegistry(
        Action<IServiceCollection> configure,
        ILogger<InboundProcessorHandlerRegistry<INatsHandler>>? logger = null)
    {
        var services = new ServiceCollection();
        configure(services);
        // Registry reads handlers only in its constructor, so the provider can be disposed right after.
        using var provider = services.BuildServiceProvider();
        return new InboundProcessorHandlerRegistry<INatsHandler>(
            provider.GetRequiredService<IServiceScopeFactory>(),
            new NatsTopicMatcher(),
            logger ?? NullLogger<InboundProcessorHandlerRegistry<INatsHandler>>.Instance);
    }

    private sealed class CapturingLogger : ILogger<InboundProcessorHandlerRegistry<INatsHandler>>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Entries.Add((logLevel, formatter(state, exception)));
    }

    private sealed class ExactHandler() : TestHandler("orders.created");

    private sealed class SingleTokenHandler() : TestHandler("orders.*");

    private sealed class TailHandler() : TestHandler("orders.>");

    private sealed class InvalidHandler() : TestHandler("orders.>.created");

    private abstract class TestHandler(string pattern) : INatsHandler
    {
        public string Topic => pattern;

        public Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
