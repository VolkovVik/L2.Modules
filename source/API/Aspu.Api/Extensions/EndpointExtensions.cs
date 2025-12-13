using Asp.Versioning;
using Aspu.Api.Adapters.Http;

namespace Aspu.Api.Extensions;

internal static class EndpointExtensions
{
    /// <remarks>
    /// https://www.milanjovanovic.tech/blog/api-versioning-in-aspnetcore
    /// </remarks>

    internal static IServiceCollection AddApiEndpoint(this IServiceCollection services)
    {
        /// services.AddEndpoints(AssemblyReference.Assembly);

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
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

    internal static IApplicationBuilder UseApiEndpoint(this WebApplication app, int[] versions)
    {
        ArgumentNullException.ThrowIfNull(app);

        var apiVersionSetBuilder = app.NewApiVersionSet();

        foreach (var version in versions)
            apiVersionSetBuilder.HasApiVersion(new ApiVersion(version));

        var apiVersionSet = apiVersionSetBuilder
            .ReportApiVersions()
            .Build();

        var routeGroupBuilder = app
            .MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(apiVersionSet);

        MainModule.MapEndpoints(app, routeGroupBuilder);

        return app;
    }
}
