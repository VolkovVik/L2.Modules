using System.Globalization;
using Aspu.Api.Options;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace Aspu.Api.Extensions;

internal static class ScalarExtensions
{
    internal static IServiceCollection AddOpenApi(this IServiceCollection services, ApiVersionOptions apiVersionOptions)
    {
        foreach (var version in apiVersionOptions.Versions)
            services.AddOpenApi($"v{version}", options => GetOptions(options, string.Create(CultureInfo.InvariantCulture, $"ASPU API Reference version {version}")));

        return services;
    }

    private static void GetOptions(OpenApiOptions options, string title)
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info.Version = "0.0.1";
            document.Info.Title = title;
            document.Info.Description = "This API demonstrates OpenAPI customization in a .NET 10 ASPU project.";
            document.Info.TermsOfService = new Uri("https://vekas-automation.ru/");
            document.Info.Contact = new OpenApiContact
            {
                Name = "Vekas",
                Email = "info@vekas-automation.ru",
                Url = new Uri("https://vekas-automation.ru/"),
            };
            return Task.CompletedTask;
        });
    }


    internal static IEndpointRouteBuilder MapScalarApiReference(this IEndpointRouteBuilder endpoints, ApiVersionOptions apiVersionOptions)
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
