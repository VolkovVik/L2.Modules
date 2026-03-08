namespace Aspu.Common.Domain.Errors;

public record Error : IError
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    public static readonly Error Default = new(
        "General.Default",
        "Default error was provided",
        ErrorType.Failure);

    public static readonly Error NullValue = new(
        "General.Null",
        "Null value was provided",
        ErrorType.Failure);

    private Error() { }

    public Error(string code, string description, ErrorType type)
    {
        Code = code;
        Type = type;
        Description = description;
    }

    /// <summary>
    ///     Код ошибки
    /// </summary>
    public string Code { get; }

    /// <summary>
    ///     Описание ошибки
    /// </summary>
    public string Description { get; }

    /// <summary>
    ///     Тип ошибки
    /// </summary>
    public ErrorType Type { get; }

    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error ValueIsInvalid(string name) =>
        Failure("value.is.invalid", $"Value is invalid for {name}");

    public static Error ValueIsRequired(string name) =>
        Failure("value.is.required", $"Value is required for {name}");
}
