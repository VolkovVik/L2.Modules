using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.InboundProcessor;
using Aspu.Common.Presentation.Abstractions.NatsAdapter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Aspu.Api.IntegrationTests.InboundProcessor;

internal sealed class InboundProcessorNatsHandlerRegistryShould
{
    [Test]
    public async Task Resolve_Exact_Topic()
    {
        var registry = CreateRegistry(services => services
            .AddInboundProcessorHandler<INatsHandler, ExactHandler>());

        var isEnabled = registry.TryResolve("orders.created", out var handlerType);

        await Assert.That(isEnabled).IsTrue();
        await Assert.That(handlerType).IsEqualTo(typeof(ExactHandler));
    }

    [Test]
    public async Task Resolve_Wildcard_Topic()
    {
        var registry = CreateRegistry(services => services
            .AddInboundProcessorHandler<INatsHandler, SingleTokenHandler>());

        var isEnabled = registry.TryResolve("orders.updated", out var handlerType);

        await Assert.That(isEnabled).IsTrue();
        await Assert.That(handlerType).IsEqualTo(typeof(SingleTokenHandler));
    }

    [Test]
    public async Task Resolve_Tail_Topic()
    {
        var registry = CreateRegistry(services => services
            .AddInboundProcessorHandler<INatsHandler, TailHandler>());

        var isEnabled = registry.TryResolve("orders.deleted.v2", out var handlerType);

        await Assert.That(isEnabled).IsTrue();
        await Assert.That(handlerType).IsEqualTo(typeof(TailHandler));
    }

    [Test]
    public async Task Resolve_Exact_And_Wildcard_Together()
    {
        var registry = CreateRegistry(services => services
            .AddInboundProcessorHandler<INatsHandler, ExactHandler>()
            .AddInboundProcessorHandler<INatsHandler, CustomersHandler>()
            .AddInboundProcessorHandler<INatsHandler, InvoicesHandler>());

        var isExactEnabled = registry.TryResolve("orders.created", out var exactHandlerType);
        var isWildcardEnabled = registry.TryResolve("invoices.paid.v2", out var wildcardHandlerType);

        await Assert.That(isExactEnabled).IsTrue();
        await Assert.That(exactHandlerType).IsEqualTo(typeof(ExactHandler));
        await Assert.That(isWildcardEnabled).IsTrue();
        await Assert.That(wildcardHandlerType).IsEqualTo(typeof(InvoicesHandler));
    }

    [Test]
    public async Task Resolve_Cached_Wildcard_Topic()
    {
        var registry = CreateRegistry(services => services
            .AddInboundProcessorHandler<INatsHandler, SingleTokenHandler>());

        registry.TryResolve("orders.updated", out _);
        var isEnabled = registry.TryResolve("orders.updated", out var handlerType);

        await Assert.That(isEnabled).IsTrue();
        await Assert.That(handlerType).IsEqualTo(typeof(SingleTokenHandler));
    }

    [Test]
    public async Task Not_Resolve_Unknown_Topic()
    {
        var registry = CreateRegistry(services => services
            .AddInboundProcessorHandler<INatsHandler, ExactHandler>()
            .AddInboundProcessorHandler<INatsHandler, CustomersHandler>());

        var isEnabled = registry.TryResolve("payments.updated", out var handlerType);

        await Assert.That(isEnabled).IsFalse();
        await Assert.That(handlerType).IsNull();
    }

    [Test]
    public async Task Throw_For_Invalid_Topic()
    {
        await Assert.That(() => CreateRegistry(services => services
                .AddInboundProcessorHandler<INatsHandler, ExactHandler>()
                .AddInboundProcessorHandler<INatsHandler, InvalidHandler>()))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Throw_For_Duplicate_Topic() =>
        await Assert.That(() => CreateRegistry(services => services
                .AddInboundProcessorHandler<INatsHandler, ExactHandler>()
                .AddInboundProcessorHandler<INatsHandler, DuplicateExactHandler>()))
            .Throws<InvalidOperationException>()
            .WithMessageContaining("duplicate", StringComparison.Ordinal);

    [Test]
    public async Task Throw_For_Exact_Topic_Overlapping_Wildcard() =>
        await Assert.That(() => CreateRegistry(services => services
                .AddInboundProcessorHandler<INatsHandler, ExactHandler>()
                .AddInboundProcessorHandler<INatsHandler, SingleTokenHandler>()))
            .Throws<InvalidOperationException>()
            .WithMessageContaining("overlapping", StringComparison.Ordinal);

    [Test]
    public async Task Throw_For_Overlapping_Wildcards() =>
        await Assert.That(() => CreateRegistry(services => services
                .AddInboundProcessorHandler<INatsHandler, SingleTokenHandler>()
                .AddInboundProcessorHandler<INatsHandler, TailHandler>()))
            .Throws<InvalidOperationException>()
            .WithMessageContaining("overlapping", StringComparison.Ordinal);

    [Test]
    public async Task Throw_For_Not_Keyed_Handler() =>
        await Assert.That(() => CreateRegistry(services => services
                .AddSingleton(new InboundProcessorTopic<INatsHandler>("orders.created", typeof(ExactHandler)))))
            .Throws<InvalidOperationException>()
            .WithMessageContaining("keyed", StringComparison.Ordinal);

    [Test]
    public async Task Return_Patterns_As_Subscriptions()
    {
        var registry = CreateRegistry(services => services
            .AddInboundProcessorHandler<INatsHandler, ExactHandler>()
            .AddInboundProcessorHandler<INatsHandler, CustomersHandler>()
            .AddInboundProcessorHandler<INatsHandler, InvoicesHandler>());

        var subscriptions = registry.GetSubscriptions();

        await Assert.That(subscriptions).IsEquivalentTo(["orders.created", "customers.*", "invoices.>"]);
    }

    [Test]
    public async Task Build_From_Registrations_Without_Creating_Handlers()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddInboundProcessor<NatsOptions, INatsHandler, NatsTopicMatcher>();
        services.AddInboundProcessorHandler<INatsHandler, ThrowingHandler>();
        // Same scope validation as the API in Development: scoped handlers can't come from the root provider.
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        var registry = provider.GetRequiredService<InboundProcessorHandlerRegistry<INatsHandler>>();

        await Assert.That(registry.GetSubscriptions()).IsEquivalentTo(["orders.created"]);
    }

    private static InboundProcessorHandlerRegistry<INatsHandler> CreateRegistry(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        // Registry reads registrations only in its constructor, so the provider can be disposed right after.
        using var provider = services.BuildServiceProvider();
        return new InboundProcessorHandlerRegistry<INatsHandler>(
            provider.GetServices<InboundProcessorTopic<INatsHandler>>(),
            provider.GetRequiredService<IServiceProviderIsKeyedService>(),
            new NatsTopicMatcher(),
            NullLogger<InboundProcessorHandlerRegistry<INatsHandler>>.Instance);
    }

    private sealed class ExactHandler : TestHandler, IInboundTopic
    {
        public static string Topic => "orders.created";
    }

    private sealed class SingleTokenHandler : TestHandler, IInboundTopic
    {
        public static string Topic => "orders.*";
    }

    private sealed class TailHandler : TestHandler, IInboundTopic
    {
        public static string Topic => "orders.>";
    }

    private sealed class DuplicateExactHandler : TestHandler, IInboundTopic
    {
        public static string Topic => "orders.created";
    }

    private sealed class CustomersHandler : TestHandler, IInboundTopic
    {
        public static string Topic => "customers.*";
    }

    private sealed class InvoicesHandler : TestHandler, IInboundTopic
    {
        public static string Topic => "invoices.>";
    }

    private sealed class InvalidHandler : TestHandler, IInboundTopic
    {
        public static string Topic => "orders.>.created";
    }

    private sealed class ThrowingHandler : TestHandler, IInboundTopic
    {
        public ThrowingHandler() => throw new InvalidOperationException("Registry must not create handlers");

        public static string Topic => "orders.created";
    }

    private abstract class TestHandler : INatsHandler
    {
        public Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
