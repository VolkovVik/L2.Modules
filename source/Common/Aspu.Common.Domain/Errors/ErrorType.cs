namespace Aspu.Common.Domain.Errors;

public enum ErrorType
{
    None = 0,
    Failure = 1,
    Unexpected = 2,
    Validation = 3,
    Problem = 4,
    NotFound = 5,
    Conflict = 6,
    Unauthorized = 7,
    Forbidden = 8,
}
