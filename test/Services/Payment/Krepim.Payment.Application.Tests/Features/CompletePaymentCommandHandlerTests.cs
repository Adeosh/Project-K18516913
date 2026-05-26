using FluentAssertions;
using Krepim.EventBus.Events.Payment;
using Krepim.Payment.Application.Features.CompletePayment;
using Krepim.Payment.Domain.Entities;
using Krepim.SharedKernel.Results;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Krepim.Payment.Application.Tests.Features
{
    public class CompletePaymentCommandHandlerTests : PaymentApplicationTestBase
    {
        private readonly CompletePaymentCommandHandler _handler;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<ILogger<CompletePaymentCommandHandler>> _loggerMock;

        public CompletePaymentCommandHandlerTests()
        {
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _loggerMock = new Mock<ILogger<CompletePaymentCommandHandler>>();

            _handler = new CompletePaymentCommandHandler(
                PaymentRepositoryMock.Object,
                UnitOfWorkMock.Object,
                _publishEndpointMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_Should_CompletePaymentAndPublishEvent_When_TransactionExistsAndPending()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var externalId = "stripe_intent_id_999";
            var transaction = PaymentTransaction.Create(orderId, 2000m).Value;
            transaction.SetExternalDetails(externalId, "https://stripe.com/url");

            PaymentRepositoryMock
                .Setup(x => x.GetByExternalIdAsync(externalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction);

            var command = new CompletePaymentCommand(externalId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            transaction.Status.Should().Be(Domain.Enums.PaymentStatus.Succeeded);

            _publishEndpointMock.Verify(x =>
                x.Publish(It.Is<PaymentSucceededIntegrationEvent>(e => e.OrderId == orderId), It.IsAny<CancellationToken>()),
                Times.Once);

            UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFound_When_TransactionDoesNotExist()
        {
            // Arrange
            var externalId = "unknown_id";
            PaymentRepositoryMock
                .Setup(x => x.GetByExternalIdAsync(externalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PaymentTransaction?)null);

            var command = new CompletePaymentCommand(externalId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.NotFound);
            result.Error.Code.Should().Be("Payment.NotFound");

            _publishEndpointMock.Verify(x => x.Publish(It.IsAny<PaymentSucceededIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Never);
            UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
