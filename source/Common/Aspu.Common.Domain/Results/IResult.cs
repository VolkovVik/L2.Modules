using Aspu.Common.Domain.Errors;

namespace Aspu.Common.Domain.Results;

public interface IResult
{
    bool IsSuccess { get; }

    bool IsFailure { get; }

    string? Description { get; }
}

public interface IResult<out TValue> : IResult
{
    TValue? Value { get; }
}

public interface IResult<out TValue, out TError> : IResult<TValue>
    where TError : IError
{
    TError? Error { get; }
}
