using Aspu.Common.Domain.Errors;
using Aspu.Common.Domain.Results;

namespace Aspu.Common.Presentation.Results;

public static class ResultExtensions
{
    extension(AppResult result)
    {
        public TOut Match<TOut>(
            Func<TOut> onSuccess,
            Func<Error, TOut> onFailure) =>
            result.IsSuccess ? onSuccess() : onFailure(result.Error);
    }

    extension<TIn>(AppResult<TIn> result)
    {
        public TOut Match<TOut>(
            Func<TIn, TOut> onSuccess,
            Func<AppResult<TIn>, TOut> onFailure) =>
            result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
    }
}
