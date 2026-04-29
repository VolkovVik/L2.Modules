namespace Aspu.Api.Options;

/// <summary>
/// API versioning and OpenAPI settings.
/// Configuration section: "ApiVersion".
/// </summary>
public sealed class ApiVersionOptions
{
    public const string SectionName = "ApiVersion";

    /// <summary>
    /// Available API versions (e.g. 1, 2).
    /// </summary>
    public int[] Versions { get; set; } = [1];

    /// <summary>
    /// Default API version when not specified in the request.
    /// </summary>
    public int DefaultVersion { get; set; } = 1;
}
