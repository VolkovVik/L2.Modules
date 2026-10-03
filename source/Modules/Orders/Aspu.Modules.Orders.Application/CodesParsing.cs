using System.Buffers;
using System.Collections.Frozen;

namespace Aspu.Modules.Orders.Application;

public static class CodesParsing
{
    private const char GroupSeparator = '\u001d';
    private const int MaxStackallocLength = 256 * 2;

    private sealed record ApplicationId(string Id, int Length, bool IsVariable = false);

    private static readonly FrozenDictionary<string, ApplicationId> _dictionary =
        new ApplicationId[]
        {
            new("00", 18),
            new("01", 14),
            new("02", 14),
            new("10", 20, IsVariable: true),
            new("11", 6),
            new("12", 6),
            new("13", 6),
            new("15", 6),
            new("17", 6),
            new("20", 2),
            new("21", 20, IsVariable: true),
            new("22", 20, IsVariable: true),
            new("240", 30, IsVariable: true),
            new("241", 30, IsVariable: true),
            new("242", 6, IsVariable: true),
            new("250", 30, IsVariable: true),
            new("251", 30, IsVariable: true),
            new("253", 30, IsVariable: true),
            new("254", 20, IsVariable: true),
            new("255", 25, IsVariable: true),
            new("30", 8, IsVariable: true),
            new("3100", 6),
            new("3101", 6),
            new("3102", 6),
            new("3103", 6),
            new("3104", 6),
            new("3105", 6),
            new("3106", 6),
            new("3350", 6),
            new("3351", 6),
            new("3352", 6),
            new("3353", 6),
            new("3354", 6),
            new("3355", 6),
            new("3356", 6),
            new("37", 8, IsVariable: true),
            new("400", 30, IsVariable: true),
            new("401", 30, IsVariable: true),
            new("402", 17),
            new("403", 30, IsVariable: true),
            new("410", 13),
            new("411", 13),
            new("412", 13),
            new("413", 13),
            new("414", 13),
            new("415", 13),
            new("420", 20, IsVariable: true),
            new("421", 12, IsVariable: true),
            new("422", 3),
            new("423", 15, IsVariable: true),
            new("424", 3),
            new("425", 15, IsVariable: true),
            new("426", 3),
            new("7001", 13),
            new("7002", 30, IsVariable: true),
            new("7003", 10),
            new("8001", 14),
            new("8002", 20, IsVariable: true),
            new("8003", 30, IsVariable: true),
            new("8004", 30, IsVariable: true),
            new("8005", 6),
            new("8006", 18),
            new("8007", 34, IsVariable: true),
            new("8008", 12, IsVariable: true),
            new("8013", 25, IsVariable: true),
            new("8017", 18),
            new("8018", 18),
            new("8020", 25, IsVariable: true),
            new("8110", 70, IsVariable: true),
            new("90", 30, IsVariable: true),
            new("91", 90, IsVariable: true),
            new("92", 90, IsVariable: true),
            new("93", 90, IsVariable: true),
            new("94", 90, IsVariable: true),
            new("95", 90, IsVariable: true),
            new("96", 90, IsVariable: true),
            new("97", 90, IsVariable: true),
            new("98", 90, IsVariable: true),
            new("99", 90, IsVariable: true),
        }.ToFrozenDictionary(x => x.Id, x => x, StringComparer.Ordinal);

    private static readonly FrozenDictionary<string, ApplicationId>.AlternateLookup<ReadOnlySpan<char>> _lookup =
        _dictionary.GetAlternateLookup<ReadOnlySpan<char>>();

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

    private static (ApplicationId? applicationId, int length) ReadElement(ReadOnlySpan<char> span)
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
        var length = Math.Min(4, code.Length - 1);
        for (var i = 2; i <= length; i++)
        {
            if (_lookup.TryGetValue(code[..i], out var value))
                return value;
        }
        return null;
    }
}
