using SourceGeneratorsLibrary;

namespace Aspu.Modules.Orders.Domain.Model.CodeAggregate;

/// <summary>
///     Marking code print status
/// </summary>
[DomainEnum]
public sealed partial class CodePrintedStatus
{
    public static CodePrintedStatus Unprinted { get; }
    public static CodePrintedStatus Printing { get; }
    public static CodePrintedStatus Printed { get; }
}
