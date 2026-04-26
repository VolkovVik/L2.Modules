using Aspu.Common.Domain.Errors;
using Aspu.Common.Domain.Results;

namespace Aspu.Common.Presentation.Results;

public static class ResultExtensions
{
    extension(Result result)
    {
        public TOut Match<TOut>(
            Func<TOut> onSuccess,
            Func<Error, TOut> onFailure) =>
            result.IsSuccess ? onSuccess() : onFailure(result.Error);
    }

    extension<TIn>(Result<TIn> result)
    {
        public TOut Match<TOut>(
            Func<TIn, TOut> onSuccess,
            Func<Result<TIn>, TOut> onFailure) =>
            result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
    }
}
