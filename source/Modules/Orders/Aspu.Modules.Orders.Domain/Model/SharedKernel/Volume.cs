using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Aspu.Common.Domain.Errors;
using CSharpFunctionalExtensions;

namespace Aspu.Modules.Orders.Domain.Model.SharedKernel;

/// <summary>
///     Volume
/// </summary>
public sealed class Volume : ValueObject
{
    /// <summary>
    ///     Ctr
    /// </summary>
    [ExcludeFromCodeCoverage]
    private Volume() { }

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="value">Value in milliliters</param>
    private Volume(int value) : this()
    {
        Value = value;
    }

    /// <summary>
    ///     Value
    /// </summary>
    public int Value { get; private set; }

    /// <summary>
    ///     Factory Method
    /// </summary>
    /// <param name="value">Value in milliliters</param>
    /// <returns>Result</returns>
    public static Common.Domain.Results.Result<Volume, Error> Create(int value)
    {
        if (value <= 0)
            return Error.ValueIsRequired(nameof(value));

        return new Volume(value);
    }

    /// <summary>
    ///     Compares two volumes
    /// </summary>
    /// <param name="first">Volume 1</param>
    /// <param name="second">Volume 2</param>
    /// <returns>Result</returns>
    public static bool operator <(Volume first, Volume second)
    {
        var result = first.Value < second.Value;
        return result;
    }

    /// <summary>
    ///     Compares two volumes
    /// </summary>
    /// <param name="first">Volume 1</param>
    /// <param name="second">Volume 2</param>
    /// <returns>Result</returns>
    public static bool operator <=(Volume first, Volume second)
    {
        var result = first.Value <= second.Value;
        return result;
    }

    /// <summary>
    ///     Compares two volumes
    /// </summary>
    /// <param name="first">Volume 1</param>
    /// <param name="second">Volume 2</param>
    /// <returns>Result</returns>
    public static bool operator >(Volume first, Volume second)
    {
        var result = first.Value > second.Value;
        return result;
    }

    /// <summary>
    ///     Compares two volumes
    /// </summary>
    /// <param name="first">Volume 1</param>
    /// <param name="second">Volume 2</param>
    /// <returns>Result</returns>
    public static bool operator >=(Volume first, Volume second)
    {
        var result = first.Value >= second.Value;
        return result;
    }

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
        var value = firstPath + secondPath;
        return value;
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
