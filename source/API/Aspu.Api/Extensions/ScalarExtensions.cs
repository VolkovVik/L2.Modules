using System.Globalization;
using Aspu.Api.Options;
using Scalar.AspNetCore;

namespace Aspu.Api.Extensions;

internal static class ScalarExtensions
{
    internal static IEndpointRouteBuilder MapScalarExtension(this IEndpointRouteBuilder endpoints, ApiVersionOptions apiVersionOptions)
    {
        endpoints.MapScalarApiReference(options =>
        {
            options.WithTitle("ASPU API Reference")
                .WithTheme(ScalarTheme.BluePlanet)
                .AddDocuments(apiVersionOptions.Versions.Select(x => string.Create(CultureInfo.InvariantCulture, $"v{x}")))
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
        return endpoints;
    }
}
