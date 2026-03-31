using Aspu.Modules.Orders.Application;

namespace Aspu.Modules.Orders.UnitTests.Application;

internal sealed class CodesParsingShould
{
    public static IEnumerable<string> TestEmptyCases()
    {
        yield return null;
        yield return "";
        yield return " ";
        yield return "   ";
        yield return "\t";
        yield return "\n";
        yield return "\r\n";
        yield return "\u001d";
        yield return "\u001d\u001d";
        yield return "\u001d \u001d";
        yield return "05147004940133770662";
    }

    public static IEnumerable<Func<(string, Dictionary<string, string>)>> TestCases()
    {
        yield return () => ("00147004940133770662",
            new Dictionary<string, string>(1, StringComparer.Ordinal) {
                { "00", "147004940133770662" },
            });
        yield return () => ("010123456789123421123456\u001d93abcd",
            new Dictionary<string, string>(3, StringComparer.Ordinal) {
                { "01", "01234567891234" },
                { "21", "123456" },
                { "93", "abcd" },
            });
        yield return () => ("010123456789123421123456\u001d91abcd\u001d920123456789",
            new Dictionary<string, string>(4, StringComparer.Ordinal) {
                { "01", "01234567891234" },
                { "21", "123456" },
                { "91", "abcd" },
                { "92", "0123456789" },
            });
        yield return () => ("0101234567891234240123456\u001d3103abcdfe",
            new Dictionary<string, string>(4, StringComparer.Ordinal) {
                { "01", "01234567891234" },
                { "240", "123456" },
                { "3103", "abcdfe" },
            });
        yield return () => ("010123456789123421123456\u001d10batch\u001d112601021727010293abcd",
            new Dictionary<string, string>(6, StringComparer.Ordinal) {
                { "01", "01234567891234" },
                { "21", "123456" },
                { "10", "batch" },
                { "11", "260102" },
                { "17", "270102" },
                { "93", "abcd" },
            });
    }

    public static IEnumerable<(string, char, char, string)> TransformTestCases()
    {
        yield return ("00147004940133770662", '(', ')', "(00)147004940133770662");
        yield return ("010123456789123421123456\u001d93abcd", '(', ')', "(01)01234567891234(21)123456(93)abcd");
        yield return ("010123456789123421123456\u001d91abcd\u001d920123456789", '[', ']', "[01]01234567891234[21]123456[91]abcd[92]0123456789");
        yield return ("0101234567891234240123456\u001d3103abcdfe", '<', '>', "<01>01234567891234<240>123456<3103>abcdfe");
        yield return ("010123456789123421123456\u001d10batch\u001d112601021727010293abcd", '{', '}', "{01}01234567891234{21}123456{10}batch{11}260102{17}270102{93}abcd");
    }

    [Test]
    [MethodDataSource(nameof(TestEmptyCases))]
    public async Task Parse_ReturnsEmpty_WhenCodeIsNullOrWhiteSpace(string code)
    {
        // Act
        var result = CodesParsing.Parse(code);

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    [MethodDataSource(nameof(TestCases))]
    public async Task GetCorrectPathValue(string code, Dictionary<string, string> items)
    {
        // Act
        var result = CodesParsing.Parse(code);

        // Assert
        await Assert.That(result)
            .Count().IsEqualTo(items.Count)
            .And
            .IsEquivalentTo(items);
    }

    [Test]
    [MethodDataSource(nameof(TestEmptyCases))]
    public async Task Transform_ReturnsEmpty_WhenCodeIsNullOrWhiteSpace(string code)
    {
        // Act
        var result = CodesParsing.Transform(code);

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    [MethodDataSource(nameof(TransformTestCases))]
    public async Task Transform_GetCorrectPathValue(string code, char begin, char end, string value)
    {
        // Act
        var result = CodesParsing.Transform(code, begin, end);

        // Assert
        await Assert.That(result)
            .IsNotNullOrWhiteSpace()
            .And.IsEqualTo(value);
    }
}
