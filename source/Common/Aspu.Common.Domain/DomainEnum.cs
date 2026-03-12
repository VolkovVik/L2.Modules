using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;

namespace Aspu.Common.Domain;

/// <summary>
///     Base class for enumerations
/// </summary>
public abstract class DomainEnum : ValueObject
{
    /// <summary>
    ///     Ctr
    /// </summary>
    [ExcludeFromCodeCoverage]
    private DomainEnum() { }

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="name">Name</param>
    protected DomainEnum(string name) : this()
    {
        Name = name;
    }

    /// <summary>
    ///     Name
    /// </summary>
    public string Name { get; init; }

    [ExcludeFromCodeCoverage]
    public override string ToString() => Name;

    [ExcludeFromCodeCoverage]
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}
