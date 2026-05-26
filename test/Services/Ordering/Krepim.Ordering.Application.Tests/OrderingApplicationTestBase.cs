using Krepim.Ordering.Domain.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using Moq;

namespace Krepim.Ordering.Application.Tests
{
    public abstract class OrderingApplicationTestBase
    {
        protected readonly Mock<IOrderRepository> OrderRepositoryMock;
        protected readonly Mock<IUnitOfWork> UnitOfWorkMock;

        protected OrderingApplicationTestBase()
        {
            OrderRepositoryMock = new Mock<IOrderRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
        }
    }
}
