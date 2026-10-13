using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.InboundProcessor;
using Aspu.Common.Presentation.Abstractions.NatsAdapter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Aspu.Api.IntegrationTests.InboundProcessor;

internal sealed class InboundProcessorHostedServiceShould
{
    [Test]
    public async Task Not_Throw_When_Handler_Constructor_Fails()
    {
        using var provider = BuildProvider(services => services.AddScoped<FailingDependency>());

        using var service = CreateService(provider);

        await Assert.That(() => service.ProcessOneAsync(CreateMessage(), CancellationToken.None))
            .ThrowsNothing();
    }

    [Test]
    public async Task Not_Throw_When_Handler_Fails()
    {
        using var provider = BuildProvider(services => services.AddScoped<FailingDependency>(_ => new FailingDependency(throwOnCreate: false)));

        using var service = CreateService(provider);

        await Assert.That(() => service.ProcessOneAsync(CreateMessage(), CancellationToken.None))
            .ThrowsNothing();
    }

    [Test]
    [Arguments(TestHandler.HandlerTopic, new byte[0], "empty payload")]
    [Arguments(" ", new byte[] { 1 }, "empty topic")]
    public async Task Log_Dropped_Message(string topic, byte[] payload, string reason)
    {
        using var provider = BuildProvider(services => services.AddScoped<FailingDependency>(_ => new FailingDependency(throwOnCreate: false)));
        var logger = new CapturingLogger();
        using var service = CreateService(provider, logger);

        await service.ProcessOneAsync(new InboundProcessorMessage { Topic = topic, Payload = payload }, CancellationToken.None);

        // Only the drop is logged: the handler (which always throws) isn't reached.
        await Assert.That(logger.Entries).Count().IsEqualTo(1);
        await Assert.That(logger.Entries[0].EventId).IsEqualTo(10);
        await Assert.That(logger.Entries[0].Level).IsEqualTo(LogLevel.Warning);
        await Assert.That(logger.Entries[0].Message).Contains(reason, StringComparison.Ordinal);
    }

    private static ServiceProvider BuildProvider(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        services.AddInboundProcessorHandler<INatsHandler, TestHandler>(TestHandler.HandlerTopic);
        return services.BuildServiceProvider();
    }

    private static InboundProcessorHostedService<NatsOptions, INatsHandler> CreateService(
        ServiceProvider provider,
        ILogger<InboundProcessorHostedService<NatsOptions, INatsHandler>>? logger = null)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new NatsOptions());
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        // Registry reads topics from registrations only, so a failing handler constructor doesn't affect it.
        var registry = new InboundProcessorHandlerRegistry<INatsHandler>(
            provider.GetServices<InboundProcessorTopic<INatsHandler>>(),
            provider.GetRequiredService<IServiceProviderIsKeyedService>(),
            new NatsTopicMatcher(),
            NullLogger<InboundProcessorHandlerRegistry<INatsHandler>>.Instance);

        return new InboundProcessorHostedService<NatsOptions, INatsHandler>(
            options,
            scopeFactory,
            new InboundProcessorChannel<NatsOptions>(options),
            registry,
            logger ?? NullLogger<InboundProcessorHostedService<NatsOptions, INatsHandler>>.Instance);
    }

    private sealed class CapturingLogger : ILogger<InboundProcessorHostedService<NatsOptions, INatsHandler>>
    {
        public List<(int EventId, LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Entries.Add((eventId.Id, logLevel, formatter(state, exception)));
    }

    private static InboundProcessorMessage CreateMessage() =>
        new() { Topic = TestHandler.HandlerTopic, Payload = [1] };

    private sealed class FailingDependency
    {
        public FailingDependency() : this(throwOnCreate: true) { }

        public FailingDependency(bool throwOnCreate)
        {
            if (throwOnCreate)
                throw new InvalidOperationException("Dependency can't be created");
        }
    }

    private sealed class TestHandler(FailingDependency dependency) : INatsHandler
    {
        public FailingDependency Dependency { get; } = dependency;

        public const string HandlerTopic = "orders.created";

        public string Topic => HandlerTopic;

        public Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Handler failed");
    }
}
