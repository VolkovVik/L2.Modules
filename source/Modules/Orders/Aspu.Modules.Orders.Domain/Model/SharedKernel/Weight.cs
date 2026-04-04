using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Aspu.Common.Domain.Errors;
using CSharpFunctionalExtensions;

namespace Aspu.Modules.Orders.Domain.Model.SharedKernel;

/// <summary>
///     Weight
/// </summary>
public sealed class Weight : ValueObject
{
    /// <summary>
    ///     Ctr
    /// </summary>
    [ExcludeFromCodeCoverage]
    private Weight() { }

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="value">Value in grams</param>
    private Weight(int value) : this()
    {
        Value = value;
    }

    /// <summary>
    ///     Value
    /// </summary>
    public int Value { get; }

    /// <summary>
    ///     Factory Method
    /// </summary>
    /// <param name="value">Value in grams</param>
    /// <returns>Result</returns>
    public static Common.Domain.Results.Result<Weight, Error> Create(int value)
    {
        if (value <= 0)
            return Error.ValueIsRequired(nameof(value));

        return new Weight(value);
    }

    /// <summary>
    ///     Compares two weights
    /// </summary>
    /// <param name="first">Weight 1</param>
    /// <param name="second">Weight 2</param>
    /// <returns>Result</returns>
    public static bool operator <(Weight first, Weight second) =>
        first.Value < second.Value;

    /// <summary>
    ///     Compares two weights
    /// </summary>
    /// <param name="first">Weight 1</param>
    /// <param name="second">Weight 2</param>
    /// <returns>Result</returns>
    public static bool operator <=(Weight first, Weight second) =>
        first.Value <= second.Value;

    /// <summary>
    ///     Compares two weights
    /// </summary>
    /// <param name="first">Weight 1</param>
    /// <param name="second">Weight 2</param>
    /// <returns>Result</returns>
    public static bool operator >(Weight first, Weight second) =>
        first.Value > second.Value;

    /// <summary>
    ///     Compares two weights
    /// </summary>
    /// <param name="first">Weight 1</param>
    /// <param name="second">Weight 2</param>
    /// <returns>Result</returns>
    public static bool operator >=(Weight first, Weight second) =>
        first.Value >= second.Value;

    /// <summary>
    ///     Gets the value encoded in the marking code
    /// </summary>
    /// <param name="decimalPlaces"></param>
    /// <returns></returns>
    public string GetPathValue(int decimalPlaces = 3)
    {
        var shift = decimalPlaces == 3 ? 1 : Math.Pow(10, 3 - decimalPlaces);
        var firstPath = decimalPlaces.ToString("D", CultureInfo.CurrentCulture);
        var secondPath = ((int)(Value / shift)).ToString("D", CultureInfo.CurrentCulture).PadLeft(6, '0');
        return firstPath + secondPath;
    }

    /// <summary>
    ///     Overload for identity determination
    /// </summary>
    /// <returns>Result</returns>
    /// <remarks>Identity is determined by the set of fields specified in the method</remarks>
    [ExcludeFromCodeCoverage]
    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }
}
