using Aspu.Common.Domain.Errors;
using Microsoft.AspNetCore.Http;

namespace Aspu.Common.Presentation.Results;

public static class ApiResults
{
    public static IResult Problem<TValue, TError>(Domain.Results.Result<TValue, TError> result)
        where TError : IError
    {
        if (result.IsSuccess)
            throw new InvalidOperationException();

        return Microsoft.AspNetCore.Http.Results.Problem(
            detail: GetDetail(result.Error),
            statusCode: GetStatusCode(result.Error.Type),
            title: GetTitle(result.Error),
            type: GetType(result.Error.Type));
    }

    private static string GetTitle(IError error) =>
        error.Type switch
        {
            ErrorType.Failure => error.Code,
            ErrorType.Validation => error.Code,
            ErrorType.Problem => error.Code,
            ErrorType.NotFound => error.Code,
            ErrorType.Conflict => error.Code,
            _ => "Server failure",
        };

    private static string GetDetail(IError error) =>
        error.Type switch
        {
            ErrorType.Failure => error.Description,
            ErrorType.Validation => error.Description,
            ErrorType.Problem => error.Description,
            ErrorType.NotFound => error.Description,
            ErrorType.Conflict => error.Description,
            _ => "An unexpected error occurred",
        };

    private static string GetType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Failure => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            ErrorType.Problem => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        };

    private static int GetStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Failure => StatusCodes.Status400BadRequest,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Problem => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };
}
