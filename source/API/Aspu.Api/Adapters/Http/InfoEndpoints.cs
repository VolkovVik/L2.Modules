using System.Reflection;
using Aspu.Common.Presentation.Endpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Aspu.Api.Adapters.Http;

public sealed class InfoEndpoints : IEndpoint
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
    }
}
