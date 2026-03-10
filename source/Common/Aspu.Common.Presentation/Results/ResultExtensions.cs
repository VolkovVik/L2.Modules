using Aspu.Common.Domain.Errors;
using Aspu.Common.Domain.Results;

namespace Aspu.Common.Presentation.Results;

public static class ResultExtensions
{
    public static TOut Match<TIn, TOut>(
        this IResult<TIn, Error> result,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure) =>
        result.IsSuccess ? onSuccess(result.Value!) : onFailure(result.Error!);
}
