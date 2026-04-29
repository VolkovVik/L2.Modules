using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Aspu.Api.Extensions;

public sealed class OpenApiVersionDocumentTransformer : IOpenApiDocumentTransformer
{
    private const string UriString = "https://site.ru/";

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        // Получаем версию API, для которой сейчас генерируется документ
        var version = context.DocumentName;

        document.Info.Version = "1.0.0";
        document.Info.Title = $"ASPU API Reference version {version}";
        document.Info.Description = "This API demonstrates OpenAPI customization in a .NET 10 ASPU project";
        document.Info.TermsOfService = new Uri(UriString);
        document.Info.Contact = new OpenApiContact
        {
            Name = "Volkov Viktor",
            Email = "volkov.vik1@gmail.com",
            Url = new Uri(UriString),
        };

        return Task.CompletedTask;
    }
}
