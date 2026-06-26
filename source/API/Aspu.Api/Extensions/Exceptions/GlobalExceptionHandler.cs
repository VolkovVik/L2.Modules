namespace Aspu.Api.Extensions.Exceptions;

internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) :
    GenericExceptionHandler<Exception>(problemDetailsService, environment, logger)
{
    protected override int ProblemDetailStatus { get; set; } = StatusCodes.Status500InternalServerError;
    protected override string? ProblemDetailTitle { get; set; } = "Server failure";
    protected override string? ProblemDetailType { get; set; } = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
}
