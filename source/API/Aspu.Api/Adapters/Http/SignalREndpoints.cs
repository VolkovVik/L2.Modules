using Aspu.Api.Adapters.SignalR;
using Aspu.Common.Presentation.Abstractions.HttpAdapter;

namespace Aspu.Api.Adapters.Http;

internal sealed class SignalREndpoints : IHttpEndpoint
{
    public string Tags => "SignalR";

    public void MapEndpoint(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/signalr", static async (
            INotificationPublisher publisher,
            CancellationToken cancellationToken) =>
        {
            var payload = new Test1SignalrMessage("Test description", DateTime.UtcNow);
            await publisher.PublishAsync(payload, cancellationToken);
        })
            .WithName("GetSignalRTest")
            .WithSummary("Get signalr test")
            .WithDescription("Returns SignalR publish")
            .MapToApiVersion(1)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        routes.MapGet("/channel", static async (
            SignalrMessageChannel channel,
            CancellationToken cancellationToken) =>
        {
            var payload = new Test2SignalrMessage("Error description", 100, DateTime.UtcNow);
            await channel.Writer.WriteAsync(payload, cancellationToken);
        })
            .WithName("GetChannelTest")
            .WithSummary("Get channel test")
            .WithDescription("Returns channel publish")
            .MapToApiVersion(1)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}
