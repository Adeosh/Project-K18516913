using FluentAssertions;
using Krepim.EventBus.Events.Payment;
using Krepim.Payment.Application.Features.ProcessPayment;
using Krepim.Payment.Domain.Entities;
using Krepim.SharedKernel.Enums;
using Krepim.SharedKernel.Results;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Krepim.Payment.Application.Tests.Features
{
    public class ProcessPaymentWebhookCommandHandlerTests : PaymentApplicationTestBase
    {
        private readonly ProcessPaymentWebhookCommandHandler _handler;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<ILogger<ProcessPaymentWebhookCommandHandler>> _loggerMock;

        public ProcessPaymentWebhookCommandHandlerTests()
        {
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _loggerMock = new Mock<ILogger<ProcessPaymentWebhookCommandHandler>>();

            _handler = new ProcessPaymentWebhookCommandHandler(
                PaymentRepositoryMock.Object,
                UnitOfWorkMock.Object,
                _publishEndpointMock.Object,
                _loggerMock.Object);
        }


        [Fact]
        public async Task Handle_Should_ProcessStatusAndPublishEvent_When_TransactionExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var externalId = "yookassa_tx_123";
            var transaction = PaymentTransaction.Create(orderId, 2000m).Value;

            PaymentRepositoryMock
                .Setup(x => x.GetByExternalIdAsync(externalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction);

            // Теперь используем новый формат команды с передачей статуса
            var command = new ProcessPaymentWebhookCommand(externalId, PaymentStatus.Succeeded, null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            transaction.Status.Should().Be(PaymentStatus.Succeeded);

            _publishEndpointMock.Verify(x =>
                x.Publish(It.Is<PaymentStatusChangedIntegrationEvent>(e =>
                    e.OrderId == orderId && e.Status == PaymentStatus.Succeeded),
                    It.IsAny<CancellationToken>()),
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

            var command = new ProcessPaymentWebhookCommand(externalId, PaymentStatus.Succeeded, null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.NotFound);
            result.Error.Code.Should().Be("Payment.NotFound");

            _publishEndpointMock.Verify(x => x.Publish(It.IsAny<PaymentStatusChangedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Never);
            UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ProcessFailedStatus_When_TransactionExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var externalId = "yookassa_tx_fail";
            var transaction = PaymentTransaction.Create(orderId, 2000m).Value;

            PaymentRepositoryMock
                .Setup(x => x.GetByExternalIdAsync(externalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction);

            var command = new ProcessPaymentWebhookCommand(externalId, PaymentStatus.Failed, "Insufficient funds");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            transaction.Status.Should().Be(PaymentStatus.Failed);

            _publishEndpointMock.Verify(x =>
                x.Publish(It.Is<PaymentStatusChangedIntegrationEvent>(e =>
                    e.Status == PaymentStatus.Failed),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
