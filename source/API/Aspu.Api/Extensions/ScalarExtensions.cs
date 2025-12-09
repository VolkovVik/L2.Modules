using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace Aspu.Api.Extensions;

internal static class ScalarExtensions
{
    internal static IServiceCollection AddOpenApi(this IServiceCollection services, int[] versions)
    {
        foreach (var version in versions)
            services.AddOpenApi($"v{version}", options => GetOptions(options, $"ASPU API Reference version {version}"));

        return services;
    }

    private static void GetOptions(OpenApiOptions options, string title)
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info.Version = "0.0.1";
            document.Info.Title = title;
            document.Info.Description = "This API demonstrates OpenAPI customization in a .NET 9 ASPU project.";
            document.Info.TermsOfService = new Uri("https://vekas-automation.ru/");
            document.Info.Contact = new OpenApiContact
            {
                Name = "Vekas",
                Email = "info@vekas-automation.ru",
                Url = new Uri("https://vekas-automation.ru/")
            };
            return Task.CompletedTask;
        });
    }


    internal static IEndpointRouteBuilder MapScalarApiReference(this IEndpointRouteBuilder endpoints, int[] versions)
    {
        endpoints.MapScalarApiReference(options =>
        {
            options.WithTitle("ASPU API Reference")
                .WithTheme(ScalarTheme.BluePlanet)
                .AddDocuments(versions.Select(x => $"v{x}"))
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
        return endpoints;
    }
}
