using Scalar.AspNetCore;

namespace Aspu.Api.Extensions;

internal static class ScalarExtensions
{
    internal static IEndpointRouteBuilder MapScalarExtension(this IEndpointRouteBuilder app)
    {
        app.MapScalarApiReference(options =>
        {
            var descriptions = app.DescribeApiVersions();

            for (var i = 0; i < descriptions.Count; i++)
            {
                var description = descriptions[i];
                var isDefault = i == descriptions.Count - 1;

                // isDefault is used to mark the default API version in Scalar.
                // This decides which version is selected by default when users visit the Scalar UI.
                options.WithTitle("ASPU API Reference")
                    .WithTheme(ScalarTheme.BluePlanet)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                    .AddDocument(description.GroupName, description.GroupName, isDefault: isDefault);
            }
        });
        return app;
    }
}
