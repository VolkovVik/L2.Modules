using System.Buffers;
using System.Collections.Frozen;
using System.Globalization;

namespace Aspu.Modules.Orders.Application;

public static class CodesParsing
{
    private const char GroupSeparator = '\u001d';

    private sealed record ApplicationId(string Id, int Length, bool IsVariable = false) { }

    private static readonly FrozenDictionary<int, ApplicationId> _dictionary =
        new Dictionary<int, ApplicationId>(128)
    {
        {00, new ApplicationId("00", 18)},
        {01, new ApplicationId("01", 14)},
        {02, new ApplicationId("02", 14)},
        {10, new ApplicationId("10", 20, IsVariable: true)},
        {11, new ApplicationId("11", 6)},
        {12, new ApplicationId("12", 6)},
        {13, new ApplicationId("13", 6)},
        {15, new ApplicationId("15", 6)},
        {17, new ApplicationId("17", 6)},
        {20, new ApplicationId("20", 2)},
        {21, new ApplicationId("21", 20, IsVariable: true)},
        {22, new ApplicationId("22", 20, IsVariable: true)},
        {240, new ApplicationId("240", 30, IsVariable: true)},
        {241, new ApplicationId("241", 30, IsVariable: true)},
        {242, new ApplicationId("242", 6)},
        {250, new ApplicationId("250", 30, IsVariable: true)},
        {251, new ApplicationId("251", 30, IsVariable: true)},
        {253, new ApplicationId("253", 30, IsVariable: true)},
        {254, new ApplicationId("254", 20, IsVariable: true)},
        {255, new ApplicationId("255", 25, IsVariable: true)},
        {30, new ApplicationId("30", 8)},
        {3100, new ApplicationId("3100",6)},
        {3101, new ApplicationId("3101",6)},
        {3102, new ApplicationId("3102",6)},
        {3103, new ApplicationId("3103",6)},
        {3104, new ApplicationId("3104",6)},
        {3105, new ApplicationId("3105",6)},
        {3106, new ApplicationId("3106",6)},
        {3350, new ApplicationId("3350",6)},
        {3351, new ApplicationId("3351",6)},
        {3352, new ApplicationId("3352",6)},
        {3353, new ApplicationId("3353",6)},
        {3354, new ApplicationId("3354",6)},
        {3355, new ApplicationId("3355",6)},
        {3356, new ApplicationId("3356",6)},
        {37, new ApplicationId("37", 8, IsVariable: true)},
        {400, new ApplicationId("400", 30, IsVariable: true)},
        {401, new ApplicationId("401", 30, IsVariable: true)},
        {402, new ApplicationId("402", 17)},
        {403, new ApplicationId("403", 30, IsVariable: true)},
        {410, new ApplicationId("410", 13)},
        {411, new ApplicationId("411", 13)},
        {412, new ApplicationId("412", 13)},
        {413, new ApplicationId("413", 13)},
        {414, new ApplicationId("414", 13)},
        {415, new ApplicationId("415", 13)},
        {420, new ApplicationId("420", 20, IsVariable: true)},
        {421, new ApplicationId("421", 12, IsVariable: true)},
        {422, new ApplicationId("422", 3)},
        {423, new ApplicationId("423", 15, IsVariable: true)},
        {424, new ApplicationId("424", 3)},
        {425, new ApplicationId("425", 15, IsVariable: true)},
        {426, new ApplicationId("426", 3)},
        {7001, new ApplicationId("7001", 13)},
        {7002, new ApplicationId("7002", 30, IsVariable: true)},
        {7003, new ApplicationId("7003", 10)},
        {8001, new ApplicationId("8001", 14)},
        {8002, new ApplicationId("8002", 20, IsVariable: true)},
        {8003, new ApplicationId("8003", 30, IsVariable: true)},
        {8004, new ApplicationId("8004", 30, IsVariable: true)},
        {8005, new ApplicationId("8005", 6)},
        {8006, new ApplicationId("8006", 18)},
        {8007, new ApplicationId("8007", 34, IsVariable: true)},
        {8008, new ApplicationId("8008", 12, IsVariable: true)},
        {8013, new ApplicationId("8013", 30, IsVariable: true)},
        {8017, new ApplicationId("8017", 18)},
        {8018, new ApplicationId("8018", 18)},
        {8020, new ApplicationId("8020", 25)},
        {8110, new ApplicationId("8110", 70, IsVariable: true)},
        {90, new ApplicationId("90", 30, IsVariable: true)},
        {91, new ApplicationId("91", 90, IsVariable: true)},
        {92, new ApplicationId("92", 90, IsVariable: true)},
        {93, new ApplicationId("93", 90, IsVariable: true)},
        {94, new ApplicationId("94", 90, IsVariable: true)},
        {95, new ApplicationId("95", 90, IsVariable: true)},
        {96, new ApplicationId("96", 90, IsVariable: true)},
        {97, new ApplicationId("97", 90, IsVariable: true)},
        {98, new ApplicationId("98", 90, IsVariable: true)},
        {99, new ApplicationId("99", 90, IsVariable: true)},
    }.ToFrozenDictionary();

    public static IDictionary<string, string> Parse(string code)
    {
        var result = new Dictionary<string, string>(8, StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(code))
            return result;

        var span = !code.StartsWith(GroupSeparator)
            ? code.AsSpan()
            : code.AsSpan(1);

        while (!span.IsEmpty)
        {
            var value = GetApplicationId(span);
            if (value is null)
                return result;

            span = span[value.Id.Length..];
            if (!value.IsVariable)
            {
                result.Add(value.Id, span[..value.Length].ToString());
                span = span[value.Length..];
                continue;
            }
            var lenght = span.Length < value.Length ? span.Length : value.Length;
            var index = span[..lenght].IndexOf(GroupSeparator);
            if (index >= 0)
            {
                result.Add(value.Id, span[..index].ToString());
                span = span[(index + 1)..];
                continue;
            }

            result.Add(value.Id, span[..lenght].ToString());
            span = span[lenght..];
        }
        return result;
    }

    public static string Transform(string code, char begin = '(', char end = ')')
    {
        if (string.IsNullOrWhiteSpace(code))
            return string.Empty;

        var pool = ArrayPool<char>.Shared;
        var buffer = pool.Rent(code.Length + 32);
        try
        {
            return TransformInternal(buffer, code, begin, end);
        }
        finally
        {
            pool.Return(buffer);
        }
    }

    private static string TransformInternal(char[] buffer, string code, char begin, char end)
    {
        var pos = 0;
        var span = !code.StartsWith(GroupSeparator)
            ? code.AsSpan()
            : code.AsSpan(1);

        while (!span.IsEmpty)
        {
            var value = GetApplicationId(span);
            if (value is null)
                return new string(buffer, 0, pos);

            buffer[pos++] = begin;

            value.Id.AsSpan().CopyTo(buffer.AsSpan(pos));
            pos += value.Id.Length;

            buffer[pos++] = end;

            span = span[value.Id.Length..];
            if (!value.IsVariable)
            {
                span[..value.Length].CopyTo(buffer.AsSpan(pos));
                pos += value.Length;

                span = span[value.Length..];
                continue;
            }
            var length = span.Length < value.Length ? span.Length : value.Length;
            var index = span[..length].IndexOf(GroupSeparator);
            if (index >= 0)
            {
                span[..index].CopyTo(buffer.AsSpan(pos));
                pos += index;

                span = span[(index + 1)..];
                continue;
            }

            span[..length].CopyTo(buffer.AsSpan(pos));
            pos += length;

            span = span[length..];
        }
        return new string(buffer, 0, pos);
    }

    private static ApplicationId? GetApplicationId(ReadOnlySpan<char> code)
    {
        for (var i = 2; i < 5; i++)
        {
            if (int.TryParse(code[..i], CultureInfo.InvariantCulture, out var key) &&
                _dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
        }
        return null;
    }
}
