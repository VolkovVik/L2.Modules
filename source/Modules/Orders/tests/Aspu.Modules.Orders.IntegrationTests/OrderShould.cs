using Aspu.Modules.Orders.Domain.Model.SharedKernel;

namespace Aspu.Modules.Orders.IntegrationTests;

internal sealed class OrderShould
{
    [Test]
    public async Task ReturnValuesOnGetList()
    {
        //Arrange

        //Act
        var items = Grade.GetList();

        //Assert
        await Assert.That(items).IsNotEmpty();
    }
}
