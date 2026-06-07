using Aspu.Common.Application.Ports.SignalrPort;
using Aspu.Common.Presentation.Abstractions.HttpAdapter;
using Aspu.Common.SourceGenerators.Application;

namespace Aspu.Api.Adapters.Http;

internal sealed class SignalREndpoints : IHttpEndpoint
{
    public string Tags => "SignalR";

    public void MapEndpoint(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/signalr", static async (
            ISignalrNotificationPublisher publisher,
            CancellationToken cancellationToken) =>
        {
            var notification = new Test1Notification("Test description", DateTime.UtcNow);
            await publisher.PublishAsync(notification, cancellationToken);
        })
            .WithName("GetSignalRTest")
            .WithSummary("Get signalr test")
            .WithDescription("Returns SignalR publish")
            .MapToApiVersion(1)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        routes.MapGet("/channel", static async (
            ISignalrNotificationChannel channel,
            CancellationToken cancellationToken) =>
        {
            var notification = new Test2Notification("Error description", 100, DateTime.UtcNow);
            await channel.WriteAsync(notification, cancellationToken);
        })
            .WithName("GetChannelTest")
            .WithSummary("Get channel test")
            .WithDescription("Returns channel publish")
            .MapToApiVersion(1)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}
