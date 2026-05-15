using System.Reflection;
using Aspu.Api.Adapters.SignaR;
using Aspu.Common.Presentation.Abstractions.HttpAdapter;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Aspu.Api.Adapters.Http;

internal sealed class InfoEndpoints : IHttpEndpoint
{
    public string Tags => "Info";

    public void MapEndpoint(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/version", Results<Ok<string>, NotFound> () =>
        {
            var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            return version is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(version);
        })
        .WithName("GetVersion1")
        .WithSummary("Get version")
        .WithDescription("Returns API version")
        .MapToApiVersion(1);

        routes.MapGet("/version", () =>
        {
            var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
            return $"API version: {version}";
        })
        .WithName("GetVersion2")
        .WithSummary("Get version")
        .WithDescription("Returns API version")
        .MapToApiVersion(2)
        .Produces<string>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        routes.MapGet("/signalr/test", static async (
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

        routes.MapGet("/channel/test", static async (
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
