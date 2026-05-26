using FluentAssertions;
using Krepim.Payment.Application.Features.GetPaymentUrl;
using Krepim.Payment.Domain.Entities;
using Krepim.SharedKernel.Results;
using Moq;

namespace Krepim.Payment.Application.Tests.Features
{
    public class GetPaymentUrlQueryHandlerTests : PaymentApplicationTestBase
    {
        private readonly GetPaymentUrlQueryHandler _handler;

        public GetPaymentUrlQueryHandlerTests()
        {
            _handler = new GetPaymentUrlQueryHandler(PaymentRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnUrl_When_TransactionHasValidUrl()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var tx = PaymentTransaction.Create(orderId, 1000m).Value;
            tx.SetExternalDetails("ext_id", "https://checkout.krepim.pro/pay");

            PaymentRepositoryMock
                .Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tx);

            var query = new GetPaymentUrlQuery(orderId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("https://checkout.krepim.pro/pay");
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFound_When_TransactionOrUrlIsMissing()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            PaymentRepositoryMock
                .Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PaymentTransaction?)null);

            var query = new GetPaymentUrlQuery(orderId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.NotFound);
            result.Error.Code.Should().Be("Payment.UrlNotFound");
        }
    }
}
