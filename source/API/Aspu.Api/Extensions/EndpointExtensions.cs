using Asp.Versioning;
using Aspu.Api.Options;
using Aspu.Modules.Orders.Presentation;

namespace Aspu.Api.Extensions;

internal static class EndpointExtensions
{
    internal static IServiceCollection AddEndpointExtension(this IServiceCollection services, ApiVersionOptions apiVersionOptions)
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

    internal static IApplicationBuilder UseEndpointExtension(this WebApplication app, ApiVersionOptions apiVersionOptions)
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
    /// Maps local API endpoints directly and delegates module endpoints to the shared module aggregator.
    /// </summary>
    private static void MapEndpoints(
        IEndpointRouteBuilder app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        app.MapApiEndpoints(routeGroupBuilder);
        app.MapOrdersEndpoints(routeGroupBuilder);
    }
}
