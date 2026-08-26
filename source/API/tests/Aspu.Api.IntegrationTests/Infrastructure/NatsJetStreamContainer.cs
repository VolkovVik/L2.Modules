using Testcontainers.Nats;
using TUnit.Core.Interfaces;

namespace Aspu.Api.IntegrationTests.Infrastructure;

internal sealed class NatsJetStreamContainer : IAsyncInitializer, IAsyncDisposable
{
    public NatsContainer Container { get; } = new NatsBuilder("nats:latest").Build();

    public Task InitializeAsync() => Container.StartAsync();

    public ValueTask DisposeAsync() => Container.DisposeAsync();
}
