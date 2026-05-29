using Aspu.Api.Adapters.SignaR;
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
            var str = DateTime.UtcNow.ToString(System.Globalization.CultureInfo.CurrentCulture);
            await publisher.PublishAsync("ReceiveNotification", str, cancellationToken);
            return str;
        })
            .WithName("GetSignalRTest")
            .WithSummary("Get signalr test")
            .WithDescription("Returns SignalR publish")
            .MapToApiVersion(1)
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        routes.MapGet("/channel", static async (
            SignalrMessageChannel channel,
            CancellationToken cancellationToken) =>
        {
            var str = DateTime.UtcNow.ToString(System.Globalization.CultureInfo.CurrentCulture);
            await channel.Writer.WriteAsync(new SignalrMessageValue("ReceiveNotification", str, DateTime.UtcNow), cancellationToken);
            return str;
        })
            .WithName("GetChannelTest")
            .WithSummary("Get channel test")
            .WithDescription("Returns channel publish")
            .MapToApiVersion(1)
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}
