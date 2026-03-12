using Aspu.Common.Domain;
using Aspu.Common.Domain.Errors;
using Aspu.Common.Domain.Results;

namespace Aspu.Modules.Orders.Domain.Model.CodeAggregate;

/// <summary>
///     Marking code print status
/// </summary>
public sealed class CodePrintedStatus : DomainEnum
{
    public static CodePrintedStatus Unprinted => new(nameof(Unprinted).ToLowerInvariant());
    public static CodePrintedStatus Printing => new(nameof(Printing).ToLowerInvariant());
    public static CodePrintedStatus Printed => new(nameof(Printed).ToLowerInvariant());

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="name">Name</param>
    private CodePrintedStatus(string name) : base(name) { }

    /// <summary>
    ///     List of values
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<CodePrintedStatus> GetList() =>
        [Unprinted, Printing, Printed];

    /// <summary>
    ///     Gets value by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Result<CodePrintedStatus, Error> GetByName(string name)
    {
        var items = GetList();
        var value = items.SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        return value is not null ? value : Error.ValueIsInvalid(name);
    }
}
