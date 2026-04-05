using Asp.Versioning;
using Aspu.Api.Options;

namespace Aspu.Api.Extensions;

internal static class EndpointExtensions
{
    /// <summary>
    /// https://www.milanjovanovic.tech/blog/api-versioning-in-aspnetcore
    /// </summary>
    /// <param name="services"></param>
    /// <param name="apiVersionOptions"></param>
    /// <returns></returns>
    internal static IServiceCollection AddApiEndpoint(this IServiceCollection services, ApiVersionOptions apiVersionOptions)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(apiVersionOptions.DefaultVersion);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version")
            );
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });
        return services;
    }

    internal static IApplicationBuilder UseApiEndpoint(this WebApplication app, ApiVersionOptions apiVersionOptions)
    {
        ArgumentNullException.ThrowIfNull(app);

        var apiVersionSetBuilder = app.NewApiVersionSet();
        foreach (var version in apiVersionOptions.Versions)
            apiVersionSetBuilder.HasApiVersion(new ApiVersion(version));

        var apiVersionSet = apiVersionSetBuilder
            .ReportApiVersions()
            .Build();

        var routeGroupBuilder = app
            .MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(apiVersionSet);

        MapEndpoints(app, routeGroupBuilder);

        return app;
    }

    /// <summary>
    /// The API assembly generator now aggregates public endpoints from referenced presentation assemblies,
    /// so the host only needs to invoke its own generated registration once.
    /// </summary>
    private static void MapEndpoints(
        IEndpointRouteBuilder app,
        RouteGroupBuilder? routeGroupBuilder = null) =>
        app.MapApiEndpoints(routeGroupBuilder);
}
