using Aspu.Common.Domain;
using Aspu.Common.Domain.Errors;
using Aspu.Common.Domain.Results;

namespace Aspu.Modules.Orders.Domain.Model.CodeAggregate;

/// <summary>
///     Marking code aggregation status
/// </summary>
public sealed class CodeAggregatedStatus : DomainEnum
{
    public static CodeAggregatedStatus None => new(nameof(None).ToLowerInvariant());
    public static CodeAggregatedStatus Defected => new(nameof(Defected).ToLowerInvariant());
    public static CodeAggregatedStatus Validated => new(nameof(Validated).ToLowerInvariant());
    public static CodeAggregatedStatus Aggregating => new(nameof(Aggregating).ToLowerInvariant());
    public static CodeAggregatedStatus Aggregated => new(nameof(Aggregated).ToLowerInvariant());
    public static CodeAggregatedStatus Packing => new(nameof(Packing).ToLowerInvariant());
    public static CodeAggregatedStatus Packed => new(nameof(Packed).ToLowerInvariant());

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="name">Name</param>
    private CodeAggregatedStatus(string name) : base(name) { }

    /// <summary>
    ///     List of values
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<CodeAggregatedStatus> GetList() =>
        [None, Defected, Validated, Aggregating, Aggregated, Packing, Packed];

    /// <summary>
    ///     Gets value by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Result<CodeAggregatedStatus, Error> GetByName(string name)
    {
        var items = GetList();
        var value = items.SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        return value is not null ? value : Error.ValueIsInvalid(name);
    }
}
