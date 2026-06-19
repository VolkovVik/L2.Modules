using Aspu.Common.Presentation.Results;

namespace Aspu.Api.Extensions.Exceptions;

internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) :
    GenericExceptionHandler<Exception>(problemDetailsService, environment, logger)
{
    protected override string? ProblemDetailTitle { get; set; } = ProblemDetailsMappings.ServerFailureTitle;
    protected override int ProblemDetailStatus { get; set; } = StatusCodes.Status500InternalServerError;
    protected override string? ProblemDetailType { get; set; } = ProblemDetailsMappings.ServerErrorTypeUri;
}
