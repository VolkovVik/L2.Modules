namespace Aspu.Api.Extensions.Exceptions;

internal sealed class UnauthorizedAccessExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<UnauthorizedAccessExceptionHandler> logger) :
    GenericExceptionHandler<UnauthorizedAccessException>(problemDetailsService, environment, logger)
{
    protected override int ProblemDetailStatus { get; set; } = StatusCodes.Status401Unauthorized;
    protected override string? ProblemDetailTitle { get; set; } = "Unauthorized access";
    protected override string? ProblemDetailType { get; set; } = "https://tools.ietf.org/html/rfc7235#section-3.1";
    protected override string? ProblemDetailDescription { get; set; } = "Unauthorized access";
}
