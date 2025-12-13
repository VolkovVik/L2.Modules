using System.Reflection;

namespace Aspu.Api.Adapters.Http;

internal static class GetVersion1
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/version", () =>
        {
            var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
            return version;
        })
        .WithTags(Tags.Api)
        .WithName("Get version 1")
        .WithDescription("Returns API version")
        .MapToApiVersion(1);
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
        .WithTags(Tags.Api)
        .WithName("Get version 2")
        .WithDescription("Returns API version")
        .MapToApiVersion(2);
    }
}
