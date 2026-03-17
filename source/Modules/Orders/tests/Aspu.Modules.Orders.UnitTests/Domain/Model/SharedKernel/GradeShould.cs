using Aspu.Modules.Orders.Domain.Model.SharedKernel;
using Shouldly;

namespace Aspu.Modules.Orders.UnitTests.Domain.Model.SharedKernel;

internal sealed class GradeShould
{
    private static readonly Grade[] AcceptableValues = [Grade.A, Grade.B, Grade.C, Grade.D, Grade.F];

    [Test]
    public void ReturnValuesOnGetList()
    {
        //Arrange

        //Act
        var items = Grade.GetList();

        //Assert
        items.Count().ShouldBe(AcceptableValues.Length);
        items.ShouldAllBe(value => AcceptableValues.Contains(value));
    }

    [Test]
    [Arguments("A")]
    [Arguments("B")]
    [Arguments("C")]
    [Arguments("D")]
    [Arguments("F")]
    public void ReturnValueWhenNameIsNotCorrectOnGetByName(string name)
    {
        //Arrange

        //Act
        var grade = Grade.GetByName(name);

        //Assert
        grade.IsSuccess.ShouldBeTrue();
        grade.Value.ShouldNotBeNull();
        grade.Value.Name.ShouldBe(name, StringCompareShould.IgnoreCase);
    }

    [Test]
    [Arguments("E")]
    [Arguments("Z")]
    [Arguments("X")]
    public void ReturnErrorWhenNameIsNotCorrectOnGetByName(string name)
    {
        //Arrange

        //Act
        var grade = Grade.GetByName(name);

        //Assert
        grade.IsSuccess.ShouldBeFalse();
        grade.Error.ShouldNotBeNull();
    }
}
