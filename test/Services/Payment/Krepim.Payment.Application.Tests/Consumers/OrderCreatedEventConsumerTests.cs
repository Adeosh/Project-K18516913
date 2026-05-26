using FluentAssertions;
using Krepim.EventBus.Events.Inventory;
using Krepim.Payment.Application.Consumers;
using Krepim.Payment.Application.Models;
using Krepim.Payment.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Krepim.Payment.Application.Tests.Consumers
{
    public class OrderCreatedEventConsumerTests : PaymentApplicationTestBase
    {
        private readonly OrderCreatedEventConsumer _consumer;
        private readonly Mock<ILogger<OrderCreatedEventConsumer>> _loggerMock;
        private readonly Mock<ConsumeContext<OrderCreatedIntegrationEvent>> _consumeContextMock;

        public OrderCreatedEventConsumerTests()
        {
            _loggerMock = new Mock<ILogger<OrderCreatedEventConsumer>>();
            _consumeContextMock = new Mock<ConsumeContext<OrderCreatedIntegrationEvent>>();

            _consumer = new OrderCreatedEventConsumer(
                PaymentRepositoryMock.Object,
                PaymentGatewayMock.Object,
                UnitOfWorkMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Consume_Should_InitializePayment_When_TransactionDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var totalPrice = 1200.50m;
            var message = new OrderCreatedIntegrationEvent(orderId, totalPrice, new List<OrderItemPayload>());

            _consumeContextMock.Setup(x => x.Message).Returns(message);

            PaymentRepositoryMock
                .Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PaymentTransaction?)null);

            var gatewayResponse = new PaymentGatewayResponse("ext_stripe_123", "https://stripe.com/pay/123");
            PaymentGatewayMock
                .Setup(x => x.InitializePaymentAsync(orderId, totalPrice, It.IsAny<CancellationToken>()))
                .ReturnsAsync(gatewayResponse);

            PaymentTransaction? savedTransaction = null;
            PaymentRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<PaymentTransaction>(), It.IsAny<CancellationToken>()))
                .Callback<PaymentTransaction, CancellationToken>((tx, _) => savedTransaction = tx)
                .Returns(Task.CompletedTask);

            // Act
            await _consumer.Consume(_consumeContextMock.Object);

            // Assert
            savedTransaction.Should().NotBeNull();
            savedTransaction!.OrderId.Should().Be(orderId);
            savedTransaction.Amount.Should().Be(totalPrice);
            savedTransaction.ExternalPaymentId.Should().Be("ext_stripe_123");
            savedTransaction.PaymentUrl.Should().Be("https://stripe.com/pay/123");

            UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Consume_Should_Skip_When_TransactionAlreadyExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var message = new OrderCreatedIntegrationEvent(orderId, 500m, new List<OrderItemPayload>());
            _consumeContextMock.Setup(x => x.Message).Returns(message);

            var existingTx = PaymentTransaction.Create(orderId, 500m).Value;
            PaymentRepositoryMock
                .Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTx);

            // Act
            await _consumer.Consume(_consumeContextMock.Object);

            // Assert
            PaymentGatewayMock.Verify(x => x.InitializePaymentAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
            UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Consume_Should_ThrowException_When_DomainValidationFails()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var invalidPrice = -100m;
            var message = new OrderCreatedIntegrationEvent(orderId, invalidPrice, new List<OrderItemPayload>());
            _consumeContextMock.Setup(x => x.Message).Returns(message);

            PaymentRepositoryMock
                .Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PaymentTransaction?)null);

            // Act
            Func<Task> act = async () => await _consumer.Consume(_consumeContextMock.Object);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Ошибка создания платежа*");
        }
    }
}
