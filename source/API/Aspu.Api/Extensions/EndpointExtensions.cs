using Asp.Versioning;
using Asp.Versioning.Conventions;
using Aspu.Modules.Orders.Presentation;

namespace Aspu.Api.Extensions;

internal static class EndpointExtensions
{
    internal static IApiVersioningBuilder AddEndpointExtension(this IServiceCollection services) =>
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1.0);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version")
            );
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

    internal static IApplicationBuilder UseEndpointExtension(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(1.0)
            .HasApiVersion(2.0)
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
