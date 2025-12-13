using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Aspu.Api.Extensions.Exceptions;

#pragma warning disable CA1852
internal abstract class GenericExceptionHandler<TException>(
    IProblemDetailsService problemDetailsService,
    ILogger<TException> logger) :
    IExceptionHandler where TException : Exception
#pragma warning restore CA1852
{
    protected virtual string? ProblemDetailType { get; set; } = string.Empty;
    protected virtual string? ProblemDetailTitle { get; set; } = string.Empty;
    protected virtual int ProblemDetailStatus { get; set; } = StatusCodes.Status404NotFound;

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
                Detail = currentException.Message
            }
        };

        UpdateProblemDetails(context, currentException);

        /// Возможно, придется корректировать контекст
        /// await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        /// return true;

        return await problemDetailsService.TryWriteAsync(context);
    }


    protected virtual void UpdateProblemDetails(ProblemDetailsContext context, Exception exception) { }

}
