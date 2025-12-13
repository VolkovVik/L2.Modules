namespace Aspu.Api.Extensions.Exceptions;

internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<Exception> logger) :
    GenericExceptionHandler<Exception>(problemDetailsService, logger)
{
    protected override string? ProblemDetailTitle { get; set; } = "Internal server error";
    protected override int ProblemDetailStatus { get; set; } = StatusCodes.Status500InternalServerError;
    protected override string? ProblemDetailType { get; set; } = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1";
}
