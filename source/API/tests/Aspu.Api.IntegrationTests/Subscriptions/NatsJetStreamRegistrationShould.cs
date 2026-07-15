using Aspu.Api.Extensions.Subscriptions;
using Aspu.Api.IntegrationTests.Infrastructure;
using Aspu.Api.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace Aspu.Api.IntegrationTests.Subscriptions;

internal sealed class NatsJetStreamRegistrationShould
{
    [ClassDataSource<NatsJetStreamContainer>(Shared = SharedType.PerTestSession)]
    public required NatsJetStreamContainer Nats { get; init; }

    [Test]
    public async Task Resolves_INatsJSContext_And_Creates_Stream()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["Nats:Enabled"] = "true",
                ["Nats:Url"] = Nats.Container.GetConnectionString(),
                ["Nats:Name"] = "aspu-api-integration-tests",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.Configure<NatsOptions>(configuration.GetSection(NatsOptions.SectionName));
        services.AddNatsSubscriber(configuration);

        await using var provider = services.BuildServiceProvider();
        var jetStream = provider.GetRequiredService<INatsJSContext>();

        await Assert.That(jetStream).IsNotNull();
        await Assert.That(jetStream.Connection).IsNotNull();

        var streamName = $"TEST_{Guid.NewGuid():N}";
        await jetStream.CreateStreamAsync(new StreamConfig(streamName, [$"{streamName}.>"]));

        var stream = await jetStream.GetStreamAsync(streamName);
        await Assert.That(stream.Info.Config.Name).IsEqualTo(streamName);

        await jetStream.DeleteStreamAsync(streamName);
    }
}
