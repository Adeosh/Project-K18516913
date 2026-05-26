using Krepim.Inventory.Domain.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using Moq;

namespace Krepim.Inventory.Application.Tests
{
    public abstract class InventoryApplicationTestBase
    {
        protected readonly Mock<IInventoryRepository> RepositoryMock;
        protected readonly Mock<IUnitOfWork> UnitOfWorkMock;

        protected InventoryApplicationTestBase()
        {
            RepositoryMock = new Mock<IInventoryRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
        }
    }
}
