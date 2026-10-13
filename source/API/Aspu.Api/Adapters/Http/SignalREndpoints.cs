using Aspu.Common.Application.Ports.Clock;
using Aspu.Common.Application.Ports.SignalrPort;
using Aspu.Common.Presentation.Abstractions.HttpAdapter;
using Aspu.Common.SourceGenerators.Application;

namespace Aspu.Api.Adapters.Http;

internal sealed class SignalrEndpoints : IHttpEndpoint
{
    public string Tags => "SignalR";

    public void MapEndpoint(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/signalr", static async (
            ISignalrNotificationPublisher publisher,
            IDateTimeProvider dateTimeProvider,
            CancellationToken cancellationToken) =>
        {
            var notification = new Test1Notification("Test description", dateTimeProvider.UtcNow);
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
            IDateTimeProvider dateTimeProvider,
            CancellationToken cancellationToken) =>
        {
            var notification = new Test2Notification("Error description", 100, dateTimeProvider.UtcNow);
            channel.TryWrite(notification);
        })
            .WithName("GetChannelTest")
            .WithSummary("Get channel test")
            .WithDescription("Returns channel publish")
            .MapToApiVersion(1)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}
