using Aspu.Modules.Orders.Domain.Model.SharedKernel;

namespace Aspu.Modules.Orders.UnitTests.Domain.Model.SharedKernel;

internal sealed class GradeShould
{
    private static readonly Grade[] AcceptableValues =
        [Grade.A, Grade.B, Grade.C, Grade.D, Grade.F];

    public static IEnumerable<(string, Grade)> TestCases()
    {
        yield return ("A", Grade.A);
        yield return ("B", Grade.B);
        yield return ("C", Grade.C);
        yield return ("D", Grade.D);
        yield return ("F", Grade.F);
    }

    [Test]
    public async Task ReturnValuesOnGetList()
    {
        //Arrange

        //Act
        var items = Grade.GetList();

        //Assert
        await Assert.That(items).Count().IsEqualTo(AcceptableValues.Length);
        await Assert.That(items).IsEquivalentTo(AcceptableValues);
    }

    [Test]
    [MethodDataSource(nameof(TestCases))]
    public async Task ReturnValueWhenNameIsNotCorrectOnGetByName(string name, Grade grade)
    {
        //Arrange

        //Act
        var result = Grade.GetByName(name);

        //Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value)
            .IsNotNull()
            .And.IsEqualTo(grade);
    }

    [Test]
    [Arguments("E")]
    [Arguments("Z")]
    [Arguments("X")]
    public async Task ReturnErrorWhenNameIsNotCorrectOnGetByName(string name)
    {
        //Arrange

        //Act
        var grade = Grade.GetByName(name);

        //Assert
        await Assert.That(grade.IsSuccess).IsFalse();
        await Assert.That(grade.Error).IsNotNull();
    }
}
