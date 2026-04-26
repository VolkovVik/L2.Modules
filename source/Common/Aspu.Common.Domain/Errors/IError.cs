namespace Aspu.Common.Domain.Errors;

public interface IError
{
    string Code { get; }
    string Description { get; }
    ErrorType Type { get; }
}
