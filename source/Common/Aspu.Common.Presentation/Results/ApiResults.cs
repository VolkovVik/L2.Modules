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
            detail: ProblemDetailsMappings.GetDetail(result.Error),
            statusCode: ProblemDetailsMappings.GetStatusCode(result.Error.Type),
            title: ProblemDetailsMappings.GetTitle(result.Error),
            type: ProblemDetailsMappings.GetProblemType(result.Error.Type));
    }
}
