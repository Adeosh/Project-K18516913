using Krepim.Payment.Application.Interfaces;
using Krepim.Payment.Domain.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using Moq;

namespace Krepim.Payment.Application.Tests
{
    public abstract class PaymentApplicationTestBase
    {
        protected readonly Mock<IPaymentRepository> PaymentRepositoryMock;
        protected readonly Mock<IPaymentGateway> PaymentGatewayMock;
        protected readonly Mock<IUnitOfWork> UnitOfWorkMock;

        protected PaymentApplicationTestBase()
        {
            PaymentRepositoryMock = new Mock<IPaymentRepository>();
            PaymentGatewayMock = new Mock<IPaymentGateway>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
        }
    }
}
