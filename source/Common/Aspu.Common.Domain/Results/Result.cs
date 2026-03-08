using System.Diagnostics.CodeAnalysis;
using Aspu.Common.Domain.Errors;

namespace Aspu.Common.Domain.Results;

public record Result : Result<object, Error>
{
    protected Result(bool isSuccess, Error? error)
        : base(isSuccess, error, new object())
    { }

    public static Result Success() =>
       new(isSuccess: true, default);

    public static new Result Failure(Error error) =>
        new(isSuccess: false, error);

    public static implicit operator Result(Error error) =>
        Failure(error);
}

public record Result<TValue> : Result<TValue, Error>
{
    protected Result(bool isSuccess, Error? error, TValue? value)
        : base(isSuccess, error, value)
    { }

    public static new Result<TValue> Success(TValue value) =>
       new(isSuccess: true, default, value);

    public static new Result<TValue> Failure(Error error) =>
        new(isSuccess: false, error, default);

    public static implicit operator Result<TValue>(TValue value) =>
        Result<TValue>.Success(value);

    public static implicit operator Result<TValue>(Error error) =>
        Result<TValue>.Failure(error);
}

public record Result<TValue, TError> : IResult<TValue, TError>
    where TError : IError
{
    public bool IsSuccess { get; }
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

    protected Result(bool isSuccess, TError? error, TValue? value)
    {
        Error = error;
        Value = value;
        IsSuccess = isSuccess;
    }

    public static Result<TValue, TError> Success(TValue value) =>
       new(isSuccess: true, default, value);

    public static Result<TValue, TError> Failure(TError error) =>
        new(isSuccess: false, error, default);

    public static implicit operator Result<TValue, TError>(TValue value) =>
        Result<TValue, TError>.Success(value);

    public static implicit operator Result<TValue, TError>(TError error) =>
        Result<TValue, TError>.Failure(error);
}
