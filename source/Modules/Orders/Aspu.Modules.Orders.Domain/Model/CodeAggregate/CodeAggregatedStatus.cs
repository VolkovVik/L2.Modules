using Aspu.Modules.Orders.SourceGenerators.Domain;

namespace Aspu.Modules.Orders.Domain.Model.CodeAggregate;

/// <summary>
///     Marking code aggregation status
/// </summary>
[DomainEnum]
public sealed partial class CodeAggregatedStatus
{
    public static CodeAggregatedStatus None => new(nameof(None));
    public static CodeAggregatedStatus Defected => new(nameof(Defected));
    public static CodeAggregatedStatus Validated => new(nameof(Validated));
    public static CodeAggregatedStatus Aggregating => new(nameof(Aggregating));
    public static CodeAggregatedStatus Aggregated => new(nameof(Aggregated));
    public static CodeAggregatedStatus Packing => new(nameof(Packing));
    public static CodeAggregatedStatus Packed => new(nameof(Packed));
}
