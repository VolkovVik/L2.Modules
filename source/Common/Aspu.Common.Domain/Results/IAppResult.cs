using Aspu.Common.Domain.Errors;

namespace Aspu.Common.Domain.Results;

public interface IAppResult
{
    bool IsSuccess { get; }

    bool IsFailure { get; }

    string? Description { get; }
}

public interface IAppResult<out TValue> : IAppResult
{
    TValue? Value { get; }
}

public interface IAppResult<out TValue, out TError> : IAppResult<TValue>
    where TError : IError
{
    TError? Error { get; }
}
