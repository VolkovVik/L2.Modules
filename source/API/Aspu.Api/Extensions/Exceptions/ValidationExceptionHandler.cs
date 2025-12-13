using FluentValidation;

namespace Aspu.Api.Extensions.Exceptions;

internal sealed class ValidationExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ValidationException> logger) :
    GenericExceptionHandler<ValidationException>(problemDetailsService, logger)
{
    protected override string? ProblemDetailTitle { get; set; } = "Invalid validation request";
    protected override int ProblemDetailStatus { get; set; } = StatusCodes.Status400BadRequest;
    protected override string? ProblemDetailType { get; set; } = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";

    protected override void UpdateProblemDetails(ProblemDetailsContext context, Exception exception)
    {
        if (exception is not ValidationException validationException)
            return;

        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key.ToLowerInvariant(),
                g => g.Select(e => e.ErrorMessage).ToArray()
            );
        context.ProblemDetails.Extensions.Add("errors", errors);
    }
}
