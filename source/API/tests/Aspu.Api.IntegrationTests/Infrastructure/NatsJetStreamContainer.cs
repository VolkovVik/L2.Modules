using Testcontainers.Nats;
using TUnit.Core.Interfaces;

namespace Aspu.Api.IntegrationTests.Infrastructure;

internal sealed class NatsJetStreamContainer : IAsyncInitializer, IAsyncDisposable
{
    public NatsContainer Container { get; } = new NatsBuilder("nats:latest").Build();

    public async Task InitializeAsync() => await Container.StartAsync();

    public async ValueTask DisposeAsync() => await Container.DisposeAsync();
}
