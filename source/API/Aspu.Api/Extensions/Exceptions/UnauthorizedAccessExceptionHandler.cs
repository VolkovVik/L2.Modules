namespace Aspu.Api.Extensions.Exceptions;

internal sealed class UnauthorizedAccessExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<UnauthorizedAccessException> logger) :
    GenericExceptionHandler<UnauthorizedAccessException>(problemDetailsService, logger)
{
    protected override string? ProblemDetailTitle { get; set; } = "Unauthorized access";
    protected override int ProblemDetailStatus { get; set; } = StatusCodes.Status401Unauthorized;
    protected override string? ProblemDetailType { get; set; } = "https://tools.ietf.org/html/rfc7235#section-3.1";
}
