using Aspu.Modules.Orders.SourceGenerators.Domain;

namespace Aspu.Modules.Orders.Domain.Model.SharedKernel;

/// <summary>
///     Marking code quality
/// </summary>
[DomainEnum]
public sealed partial class Grade
{
    public static Grade A => new(nameof(A));
    public static Grade B => new(nameof(B));
    public static Grade C => new(nameof(C));
    public static Grade D => new(nameof(D));
    public static Grade F => new(nameof(F));
}
