using Aspu.Common.Domain;
using Aspu.Common.Domain.Errors;
using Aspu.Common.Domain.Results;

namespace Aspu.Modules.Orders.Domain.Model.SharedKernel;

/// <summary>
///     Marking code quality
/// </summary>
public sealed class Grade : DomainEnum
{
    public static Grade A => new(nameof(A).ToLowerInvariant());
    public static Grade B => new(nameof(B).ToLowerInvariant());
    public static Grade C => new(nameof(C).ToLowerInvariant());
    public static Grade D => new(nameof(D).ToLowerInvariant());
    public static Grade F => new(nameof(F).ToLowerInvariant());

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="name">Name</param>
    private Grade(string name) : base(name) { }

    /// <summary>
    ///     List of values
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<Grade> GetList() =>
        [A, B, C, D, F];

    /// <summary>
    ///     Gets value by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Result<Grade, Error> GetByName(string name)
    {
        var items = GetList();
        var value = items.SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        return value != null ? value : Error.ValueIsInvalid(name);
    }
}

