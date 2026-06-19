using Aspu.Common.Domain.Errors;
using Microsoft.AspNetCore.Http;

namespace Aspu.Common.Presentation.Results;

public static class ProblemDetailsMappings
{
    public const string UnauthorizedDetail = "Unauthorized access";
    public const string UnexpectedErrorDetail = "An unexpected error occurred";
    public const string ValidationFailedDetail = "One or more validation errors occurred";

    public const string ServerFailureTitle = "Server failure";
    public const string UnauthorizedTitle = "Unauthorized access";
    public const string ValidationFailureTitle = "Validation failed";

    public const string NotFoundTypeUri = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
    public const string ConflictTypeUri = "https://tools.ietf.org/html/rfc7231#section-6.5.8";
    public const string ServerErrorTypeUri = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
    public const string ClientErrorTypeUri = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
    public const string UnauthorizedTypeUri = "https://tools.ietf.org/html/rfc7235#section-3.1";

    public static string GetTitle(IError error) =>
        error.Type switch
        {
            ErrorType.Failure => error.Code,
            ErrorType.Validation => error.Code,
            ErrorType.Problem => error.Code,
            ErrorType.NotFound => error.Code,
            ErrorType.Conflict => error.Code,
            _ => ServerFailureTitle,
        };

    public static string GetDetail(IError error) =>
        error.Type switch
        {
            ErrorType.Failure => error.Description,
            ErrorType.Validation => error.Description,
            ErrorType.Problem => error.Description,
            ErrorType.NotFound => error.Description,
            ErrorType.Conflict => error.Description,
            _ => UnexpectedErrorDetail,
        };

    public static string GetProblemType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Failure => ClientErrorTypeUri,
            ErrorType.Validation => ClientErrorTypeUri,
            ErrorType.Problem => ClientErrorTypeUri,
            ErrorType.NotFound => NotFoundTypeUri,
            ErrorType.Conflict => ConflictTypeUri,
            _ => ServerErrorTypeUri,
        };

    public static int GetStatusCode(ErrorType errorType) =>
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
