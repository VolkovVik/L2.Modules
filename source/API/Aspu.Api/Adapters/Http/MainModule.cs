namespace Aspu.Api.Adapters.Http;

public static class MainModule
{
    public static void MapEndpoints(
        IEndpointRouteBuilder app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        var builder = routeGroupBuilder ?? app;

        GetVersion1.MapEndpoint(builder);
        GetVersion2.MapEndpoint(builder);
    }
}
