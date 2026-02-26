using System.Reflection;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Aspu.Api.Adapters.Http;

internal static class GetVersion1
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/version", Results<Ok<string>, NotFound> () =>
        {
            var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            return version is not null
                ? TypedResults.Ok(version)
                : TypedResults.NotFound();
        })
        .WithName("GetVersion1")
        .WithSummary("Get version")
        .WithDescription("Returns API version")
        .MapToApiVersion(1)
        .WithTags(Tags.Api);
    }
}

internal static class GetVersion2
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/version", () =>
        {
            var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
            return $"API version: {version}";
        })
        .WithName("GetVersion2")
        .WithSummary("Get version")
        .WithDescription("Returns API version")
        .Produces<string>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .MapToApiVersion(2)
        .WithTags(Tags.Api);
    }
}
