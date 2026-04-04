using Aspu.Modules.Orders.Domain.Model.SharedKernel;

namespace Aspu.Modules.Orders.UnitTests.Domain.Model.SharedKernel;

internal sealed class WeightShould
{
    public static IEnumerable<(int, int, string)> TestCases()
    {
        yield return (123, 1, "1000001");
        yield return (123, 2, "2000012");
        yield return (123, 3, "3000123");
        yield return (123, 4, "4001230");
        yield return (123, 5, "5012300");
        yield return (123, 6, "6123000");
        yield return (1234, 1, "1000012");
        yield return (1234, 2, "2000123");
        yield return (1234, 3, "3001234");
        yield return (1234, 4, "4012340");
        yield return (1234, 5, "5123400");
        yield return (22700, 1, "1000227");
        yield return (22700, 2, "2002270");
        yield return (22700, 3, "3022700");
        yield return (22700, 4, "4227000");
    }

    [Test]
    [Arguments(1)]
    [Arguments(100)]
    [Arguments(int.MaxValue)]
    public async Task BeCorrectWhenParamsAreCorrectOnCreated(int gramms)
    {
        //Arrange

        //Act
        var result = Weight.Create(gramms);

        //Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.Value).IsEqualTo(gramms);
    }

    [Test]
    [Arguments(0)]
    [Arguments(-100)]
    [Arguments(int.MinValue)]
    public async Task ReturnErrorWhenParamsAreNotCorrectOnCreated(int gramms)
    {
        //Arrange

        //Act
        var result = Weight.Create(gramms);

        //Assert
        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Error).IsNotNull();
    }

    [Test]
    public async Task BeEqualWhenAllPropertiesIsEqual()
    {
        //Arrange
        var first = Weight.Create(10).Value;
        var second = Weight.Create(10).Value;

        //Act
        var result = first == second;

        //Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task BeNotEqualWhenAllPropertiesIsEqual()
    {
        //Arrange
        var first = Weight.Create(10).Value;
        var second = Weight.Create(5).Value;

        //Act
        var result = first == second;

        //Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task CanCompareMoreThen()
    {
        //Arrange
        var first = Weight.Create(10).Value;
        var second = Weight.Create(5).Value;

        //Act
        var result = first > second;

        //Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task CanCompareLessThen()
    {
        //Arrange
        var first = Weight.Create(5).Value;
        var second = Weight.Create(10).Value;

        //Act
        var result = first < second;

        //Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    [MethodDataSource(nameof(TestCases))]
    public async Task GetCorrectPathValue(int grams, int decimalPlaces, string value)
    {
        //Arrange
        var weight = Weight.Create(grams).Value;

        //Act
        var result = weight.GetPathValue(decimalPlaces);

        //Assert
        await Assert.That(result).IsEqualTo(value);
    }
}
