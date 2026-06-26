using FluentValidation;

namespace Aspu.Api.Extensions.Exceptions;

internal sealed class ValidationExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<ValidationExceptionHandler> logger) :
    GenericExceptionHandler<ValidationException>(problemDetailsService, environment, logger)
{
    protected override int ProblemDetailStatus { get; set; } = StatusCodes.Status400BadRequest;
    protected override string? ProblemDetailTitle { get; set; } = "Validation failed";
    protected override string? ProblemDetailType { get; set; } = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
    protected override string? ProblemDetailDescription { get; set; } = "One or more validation errors occurred";

    protected override void UpdateProblemDetails(ProblemDetailsContext context, ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key.ToLowerInvariant(),
                g => g.Select(e => e.ErrorMessage).ToArray(),
                StringComparer.OrdinalIgnoreCase);

        context.ProblemDetails.Extensions["errors"] = errors;
    }
}
