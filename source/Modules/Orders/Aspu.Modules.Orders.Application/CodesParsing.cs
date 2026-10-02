using System.Buffers;
using System.Collections.Frozen;

namespace Aspu.Modules.Orders.Application;

public static class CodesParsing
{
    private const char GroupSeparator = '\u001d';
    private const int MaxStackallocLength = 256 * 2;

    private sealed record ApplicationId(string Id, int Length, bool IsVariable = false);

    private static readonly FrozenDictionary<int, ApplicationId> _dictionary =
        new List<ApplicationId>(128)
        {
            { new ApplicationId("00", 18) },
            { new ApplicationId("01", 14) },
            { new ApplicationId("02", 14) },
            { new ApplicationId("10", 20, IsVariable: true) },
            { new ApplicationId("11", 6) },
            { new ApplicationId("12", 6) },
            { new ApplicationId("13", 6) },
            { new ApplicationId("15", 6) },
            { new ApplicationId("17", 6) },
            { new ApplicationId("20", 2) },
            { new ApplicationId("21", 20, IsVariable: true) },
            { new ApplicationId("22", 20, IsVariable: true) },
            { new ApplicationId("240", 30, IsVariable: true) },
            { new ApplicationId("241", 30, IsVariable: true) },
            { new ApplicationId("242", 6, IsVariable: true) },
            { new ApplicationId("250", 30, IsVariable: true) },
            { new ApplicationId("251", 30, IsVariable: true) },
            { new ApplicationId("253", 30, IsVariable: true) },
            { new ApplicationId("254", 20, IsVariable: true) },
            { new ApplicationId("255", 25, IsVariable: true) },
            { new ApplicationId("30", 8, IsVariable: true) },
            { new ApplicationId("3100",6) },
            { new ApplicationId("3101",6) },
            { new ApplicationId("3102",6) },
            { new ApplicationId("3103",6) },
            { new ApplicationId("3104",6) },
            { new ApplicationId("3105",6) },
            { new ApplicationId("3106",6) },
            { new ApplicationId("3350",6) },
            { new ApplicationId("3351",6) },
            { new ApplicationId("3352",6) },
            { new ApplicationId("3353",6) },
            { new ApplicationId("3354",6) },
            { new ApplicationId("3355",6) },
            { new ApplicationId("3356",6) },
            { new ApplicationId("37", 8, IsVariable: true) },
            { new ApplicationId("400", 30, IsVariable: true) },
            { new ApplicationId("401", 30, IsVariable: true) },
            { new ApplicationId("402", 17) },
            { new ApplicationId("403", 30, IsVariable: true) },
            { new ApplicationId("410", 13) },
            { new ApplicationId("411", 13) },
            { new ApplicationId("412", 13) },
            { new ApplicationId("413", 13) },
            { new ApplicationId("414", 13) },
            { new ApplicationId("415", 13) },
            { new ApplicationId("420", 20, IsVariable: true) },
            { new ApplicationId("421", 12, IsVariable: true) },
            { new ApplicationId("422", 3) },
            { new ApplicationId("423", 15, IsVariable: true) },
            { new ApplicationId("424", 3) },
            { new ApplicationId("425", 15, IsVariable: true) },
            { new ApplicationId("426", 3) },
            { new ApplicationId("7001", 13) },
            { new ApplicationId("7002", 30, IsVariable: true) },
            { new ApplicationId("7003", 10) },
            { new ApplicationId("8001", 14) },
            { new ApplicationId("8002", 20, IsVariable: true) },
            { new ApplicationId("8003", 30, IsVariable: true) },
            { new ApplicationId("8004", 30, IsVariable: true) },
            { new ApplicationId("8005", 6) },
            { new ApplicationId("8006", 18) },
            { new ApplicationId("8007", 34, IsVariable: true) },
            { new ApplicationId("8008", 12, IsVariable: true) },
            { new ApplicationId("8013", 25, IsVariable: true) },
            { new ApplicationId("8017", 18) },
            { new ApplicationId("8018", 18) },
            { new ApplicationId("8020", 25, IsVariable: true) },
            { new ApplicationId("8110", 70, IsVariable: true) },
            { new ApplicationId("90", 30, IsVariable: true) },
            { new ApplicationId("91", 90, IsVariable: true) },
            { new ApplicationId("92", 90, IsVariable: true) },
            { new ApplicationId("93", 90, IsVariable: true) },
            { new ApplicationId("94", 90, IsVariable: true) },
            { new ApplicationId("95", 90, IsVariable: true) },
            { new ApplicationId("96", 90, IsVariable: true) },
            { new ApplicationId("97", 90, IsVariable: true) },
            { new ApplicationId("98", 90, IsVariable: true) },
            { new ApplicationId("99", 90, IsVariable: true) },
        }.ToFrozenDictionary(x => int.Parse(x.Id, System.Globalization.CultureInfo.InvariantCulture), x => x);

    public static IDictionary<string, string> Parse(string code)
    {
        var result = new Dictionary<string, string>(8, StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(code))
            return result;

        var span = code.AsSpan();

        while (!span.IsEmpty)
        {
            span = span.StartsWith(GroupSeparator) ? span[1..] : span;

            var (applicationId, length) = ReadElement(span);
            if (applicationId is null)
                return result;

            span = span[applicationId.Id.Length..];

            result.TryAdd(applicationId.Id, span[..length].ToString());

            span = span[length..];
        }
        return result;
    }

    public static string Transform(string code, char begin = '(', char end = ')')
    {
        if (string.IsNullOrWhiteSpace(code))
            return string.Empty;

        // Each AI consumes at least 2 input chars and adds 2 brackets, so the output never exceeds twice the input
        var size = code.Length * 2;
        if (size <= MaxStackallocLength)
            return TransformInternal(stackalloc char[size], code, begin, end);

        var pool = ArrayPool<char>.Shared;
        var buffer = pool.Rent(size);
        try
        {
            return TransformInternal(buffer, code, begin, end);
        }
        finally
        {
            pool.Return(buffer);
        }
    }

    private static string TransformInternal(Span<char> buffer, string code, char begin, char end)
    {
        var pos = 0;
        var span = code.AsSpan();

        while (!span.IsEmpty)
        {
            span = span.StartsWith(GroupSeparator) ? span[1..] : span;

            var (applicationId, length) = ReadElement(span);
            if (applicationId is null)
                return new string(buffer[..pos]);

            span = span[applicationId.Id.Length..];

            buffer[pos++] = begin;
            applicationId.Id.CopyTo(buffer[pos..]);
            pos += applicationId.Id.Length;
            buffer[pos++] = end;
            span[..length].CopyTo(buffer[pos..]);
            pos += length;

            span = span[length..];
        }

        return new string(buffer[..pos]);
    }

    private static (ApplicationId? id, int length) ReadElement(ReadOnlySpan<char> span)
    {
        var applicationId = GetApplicationId(span);
        if (applicationId is null)
            return (null, 0);

        span = span[applicationId.Id.Length..];
        var length = Math.Min(span.Length, applicationId.Length);
        if (!applicationId.IsVariable)
            return (applicationId.Length != length ? null : applicationId, length);

        var index = span[..length].IndexOf(GroupSeparator);
        return index >= 0
            ? (applicationId, index)
            : (applicationId, length);
    }

    private static ApplicationId? GetApplicationId(ReadOnlySpan<char> code)
    {
        var key = 0;
        var length = Math.Min(4, code.Length);
        for (var i = 0; i < length; i++)
        {
            if (!char.IsAsciiDigit(code[i]))
                return null;

            key = key * 10 + (code[i] - '0');

            if (i > 0 && _dictionary.TryGetValue(key, out var value) && value.Id.Length == i + 1)
                return value;
        }
        return null;
    }
}
