using System.Diagnostics.CodeAnalysis;
using Aspu.Common.Domain.Errors;

namespace Aspu.Common.Domain.Results;

public record AppResult : AppResult<object, Error>
{
    protected AppResult(bool isSuccess, Error? error)
        : base(isSuccess, error, new object())
    { }

    public static AppResult Success() =>
       new(isSuccess: true, default);

    public static new AppResult Failure(Error error) =>
        new(isSuccess: false, error);

    public static implicit operator AppResult(Error error) =>
        Failure(error);
}

public record AppResult<TValue> : AppResult<TValue, Error>
{
    protected AppResult(bool isSuccess, Error? error, TValue? value)
        : base(isSuccess, error, value)
    { }

    public static new AppResult<TValue> Success(TValue value) =>
       new(isSuccess: true, default, value);

    public static new AppResult<TValue> Failure(Error error) =>
        new(isSuccess: false, error, default);

    public static implicit operator AppResult<TValue>(TValue value) =>
        AppResult<TValue>.Success(value);

    public static implicit operator AppResult<TValue>(Error error) =>
        AppResult<TValue>.Failure(error);
}

public record AppResult<TValue, TError> : IResult<TValue, TError>
    where TError : IError
{
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess { get; }

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    [NotNull]
    [AllowNull]
    public TValue Value => IsSuccess
        ? field!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    [NotNull]
    [AllowNull]
    public TError Error => IsFailure
        ? field!
        : throw new InvalidOperationException("The error of a failure result can't be accessed.");

    public string? Description => IsFailure ? Error.Description : string.Empty;

    protected AppResult(bool isSuccess, TError? error, TValue? value)
    {
        Error = error;
        Value = value;
        IsSuccess = isSuccess;
    }

    public static AppResult<TValue, TError> Success(TValue value) =>
       new(isSuccess: true, default, value);

    public static AppResult<TValue, TError> Failure(TError error) =>
        new(isSuccess: false, error, default);

    public static implicit operator AppResult<TValue, TError>(TValue value) =>
        AppResult<TValue, TError>.Success(value);

    public static implicit operator AppResult<TValue, TError>(TError error) =>
        AppResult<TValue, TError>.Failure(error);
}
