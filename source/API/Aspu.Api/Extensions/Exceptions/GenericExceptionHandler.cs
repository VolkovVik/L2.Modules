using Aspu.Common.Presentation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Aspu.Api.Extensions.Exceptions;

#pragma warning disable CA1852
internal abstract class GenericExceptionHandler<TException>(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger logger) :
    IExceptionHandler where TException : Exception
#pragma warning restore CA1852
{
    protected virtual int ProblemDetailStatus { get; set; } = StatusCodes.Status404NotFound;
    protected virtual string? ProblemDetailType { get; set; } = string.Empty;
    protected virtual string? ProblemDetailTitle { get; set; } = string.Empty;
    protected virtual string? ProblemDetailDescription { get; set; } = ProblemDetailsMappings.UnexpectedErrorDetail;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not TException currentException)
            return false;

        logger.LogError(
            currentException,
            "{@Exception} occurred: {@Message}",
            typeof(TException).Name,
            currentException.Message);

        httpContext.Response.StatusCode = ProblemDetailStatus;

        var context = new ProblemDetailsContext
        {
            Exception = exception,
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Type = ProblemDetailType,
                Title = ProblemDetailTitle,
                Status = ProblemDetailStatus,
                Detail = environment.IsDevelopment() ? exception.Message : ProblemDetailDescription,
            },
        };

        UpdateProblemDetails(context, currentException);

        /// Context may need to be adjusted
        /// await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        /// return true;

        return await problemDetailsService.TryWriteAsync(context);
    }

    protected virtual void UpdateProblemDetails(ProblemDetailsContext context, TException exception) { }
}
