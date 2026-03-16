using Aspu.Modules.Orders.Domain.SourceGeneratorsLibrary;

namespace Aspu.Modules.Orders.Domain.Model.CodeAggregate;

/// <summary>
///     Marking code print status
/// </summary>
[DomainEnum]
public sealed partial class CodePrintedStatus
{
    public static CodePrintedStatus Unprinted => new(nameof(Unprinted));
    public static CodePrintedStatus Printing => new(nameof(Printing));
    public static CodePrintedStatus Printed => new(nameof(Printed));
}
