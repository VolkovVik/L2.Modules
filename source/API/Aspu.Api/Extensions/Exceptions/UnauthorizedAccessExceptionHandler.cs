using Aspu.Common.Presentation.Results;

namespace Aspu.Api.Extensions.Exceptions;

internal sealed class UnauthorizedAccessExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<UnauthorizedAccessExceptionHandler> logger) :
    GenericExceptionHandler<UnauthorizedAccessException>(problemDetailsService, environment, logger)
{
    protected override int ProblemDetailStatus { get; set; } = StatusCodes.Status401Unauthorized;
    protected override string? ProblemDetailTitle { get; set; } = ProblemDetailsMappings.UnauthorizedTitle;
    protected override string? ProblemDetailType { get; set; } = ProblemDetailsMappings.UnauthorizedTypeUri;
    protected override string? ProblemDetailDescription { get; set; } = ProblemDetailsMappings.UnauthorizedDetail;
}
