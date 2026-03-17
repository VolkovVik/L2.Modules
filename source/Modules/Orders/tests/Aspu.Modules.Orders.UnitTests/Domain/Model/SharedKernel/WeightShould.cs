using Aspu.Modules.Orders.Domain.Model.SharedKernel;
using Shouldly;

namespace Aspu.Modules.Orders.UnitTests.Domain.Model.SharedKernel;

internal sealed class WeightShould
{
    [Test]
    [Arguments(1)]
    [Arguments(100)]
    [Arguments(int.MaxValue)]
    public void BeCorrectWhenParamsAreCorrectOnCreated(int gramms)
    {
        //Arrange

        //Act
        var weight = Weight.Create(gramms);

        //Assert
        weight.IsSuccess.ShouldBeTrue();
        weight.Value.Value.ShouldBe(gramms);
    }

    [Test]
    [Arguments(0)]
    [Arguments(-100)]
    [Arguments(int.MinValue)]
    public void ReturnErrorWhenParamsAreNotCorrectOnCreated(int gramms)
    {
        //Arrange

        //Act
        var weight = Weight.Create(gramms);

        //Assert
        weight.IsSuccess.ShouldBeFalse();
        weight.Error.ShouldNotBeNull();
    }

    [Test]
    public void BeEqualWhenAllPropertiesIsEqual()
    {
        //Arrange
        var first = Weight.Create(10).Value;
        var second = Weight.Create(10).Value;

        //Act
        var result = first == second;

        //Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void BeNotEqualWhenAllPropertiesIsEqual()
    {
        //Arrange
        var first = Weight.Create(10).Value;
        var second = Weight.Create(5).Value;

        //Act
        var result = first == second;

        //Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void CanCompareMoreThen()
    {
        //Arrange
        var first = Weight.Create(10).Value;
        var second = Weight.Create(5).Value;

        //Act
        var result = first > second;

        //Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void CanCompareLessThen()
    {
        //Arrange
        var first = Weight.Create(5).Value;
        var second = Weight.Create(10).Value;

        //Act
        var result = first < second;

        //Assert
        result.ShouldBeTrue();
    }

    [Test]
    [Arguments(123, 1, "1000001")]
    [Arguments(123, 2, "2000012")]
    [Arguments(123, 3, "3000123")]
    [Arguments(123, 4, "4001230")]
    [Arguments(123, 5, "5012300")]
    [Arguments(123, 6, "6123000")]
    [Arguments(1234, 1, "1000012")]
    [Arguments(1234, 2, "2000123")]
    [Arguments(1234, 3, "3001234")]
    [Arguments(1234, 4, "4012340")]
    [Arguments(1234, 5, "5123400")]
    [Arguments(22700, 1, "1000227")]
    [Arguments(22700, 2, "2002270")]
    [Arguments(22700, 3, "3022700")]
    [Arguments(22700, 4, "4227000")]
    public void GetCorrectPathValue(int grams, int decimalPlaces, string value)
    {
        //Arrange
        var weight = Weight.Create(grams).Value;

        //Act
        var result = weight.GetPathValue(decimalPlaces);

        //Assert
        result.ShouldBe(value);
    }
}
