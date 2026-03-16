using SourceGeneratorsLibrary;

namespace Aspu.Modules.Orders.Domain.Model.CodeAggregate;

/// <summary>
///     Marking code aggregation status
/// </summary>
[DomainEnum]
public sealed partial class CodeAggregatedStatus
{
    public static CodeAggregatedStatus None { get; }
    public static CodeAggregatedStatus Defected { get; }
    public static CodeAggregatedStatus Validated { get; }
    public static CodeAggregatedStatus Aggregating { get; }
    public static CodeAggregatedStatus Aggregated { get; }
    public static CodeAggregatedStatus Packing { get; }
    public static CodeAggregatedStatus Packed { get; }
}
