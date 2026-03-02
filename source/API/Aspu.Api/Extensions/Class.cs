using System.Globalization;
using Aspu.Api.Adapters.Http;
using Aspu.Common.Presentation.Endpoints;

namespace Aspu.Api.Extensions;

public static class EndpointsRegistrationGenerator111
{
    public static IEndpointRouteBuilder MapEndpoints(
        IEndpointRouteBuilder app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        var builder = routeGroupBuilder ?? app;

        MapEndpoints(new InfoEndpoints(), builder);
        MapEndpoints(new Ping1Request(), builder);
        MapEndpoints(new Ping2Request(), builder);
        MapEndpoints(new Ping3Request(), builder);
        MapEndpoints(new Ping4Request(), builder);

        return builder;
    }

#pragma warning disable S3241 // Methods should not return values that are never used
    private static IEndpointRouteBuilder MapEndpoints(
#pragma warning restore S3241 // Methods should not return values that are never used
        IEndpoint module,
        IEndpointRouteBuilder builder)
    {
        var group = string.IsNullOrWhiteSpace(module.Tags)
            ? builder
            : builder
                .MapGroup(GetGroup(module.Tags))
                .WithTags(GetTags(module.Tags));
        module.MapEndpoint(group);
        return group;
    }

    private static string GetTags(string input) =>
        string.IsNullOrWhiteSpace(input)
            ? input
            : string.Create(input.Length, input, (span, src) =>
            {
                span[0] = char.ToUpperInvariant(src[0]);
                for (var i = 1; i < src.Length; i++)
                    span[i] = char.ToLowerInvariant(src[i]);
            });

    private static string GetGroup(string input) =>
        string.IsNullOrWhiteSpace(input)
            ? input
            : $"/{input.ToLower(CultureInfo.InvariantCulture)}";
}
