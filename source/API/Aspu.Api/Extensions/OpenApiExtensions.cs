using Asp.Versioning;
using Asp.Versioning.OpenApi;

namespace Aspu.Api.Extensions;

internal static class OpenApiExtensions
{
    internal static IApiVersioningBuilder AddOpenApiExtension(this IApiVersioningBuilder services)
    {
        services.AddOpenApi((VersionedOpenApiOptions options) =>
            options.Document.AddDocumentTransformer<OpenApiVersionDocumentTransformer>());

        return services;
    }

    internal static IEndpointRouteBuilder MapOpenApiExtension(this IEndpointRouteBuilder app)
    {
        app.MapOpenApi().WithDocumentPerVersion();

        return app;
    }
}
