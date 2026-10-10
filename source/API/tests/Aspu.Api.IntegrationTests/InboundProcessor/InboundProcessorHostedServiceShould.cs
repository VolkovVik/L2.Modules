using Aspu.Api.Options;
using Aspu.Common.Presentation.Abstractions.InboundProcessor;
using Aspu.Common.Presentation.Abstractions.NatsAdapter;
using Microsoft.Extensions.DependencyInjection;
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

    private static ServiceProvider BuildProvider(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        services.AddInboundProcessorHandler<INatsHandler, TestHandler>();
        return services.BuildServiceProvider();
    }

    private static InboundProcessorHostedService<NatsOptions, INatsHandler> CreateService(ServiceProvider provider)
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
            NullLogger<InboundProcessorHostedService<NatsOptions, INatsHandler>>.Instance);
    }

    private static InboundProcessorMessage CreateMessage() =>
        new() { Type = "Nats", Topic = TestHandler.Topic, Payload = [1] };

    private sealed class FailingDependency
    {
        public FailingDependency() : this(throwOnCreate: true) { }

        public FailingDependency(bool throwOnCreate)
        {
            if (throwOnCreate)
                throw new InvalidOperationException("Dependency can't be created");
        }
    }

    private sealed class TestHandler(FailingDependency dependency) : INatsHandler, IInboundTopic
    {
        public FailingDependency Dependency { get; } = dependency;

        public static string Topic => "orders.created";

        public Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Handler failed");
    }
}
