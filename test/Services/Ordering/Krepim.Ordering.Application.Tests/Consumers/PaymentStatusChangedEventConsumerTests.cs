using FluentAssertions;
using Krepim.EventBus.Events.Payment;
using Krepim.Ordering.Application.Consumers;
using Krepim.Ordering.Domain.Entities;
using Krepim.SharedKernel.Enums;
using Krepim.SharedKernel.ValueObjects;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Krepim.Ordering.Application.Tests.Consumers
{
    public class PaymentStatusChangedEventConsumerTests : OrderingApplicationTestBase
    {
        private readonly PaymentStatusChangedEventConsumer _consumer;
        private readonly Mock<ConsumeContext<PaymentStatusChangedIntegrationEvent>> _consumeContextMock;

        public PaymentStatusChangedEventConsumerTests()
        {
            var loggerMock = new Mock<ILogger<PaymentStatusChangedEventConsumer>>();
            _consumeContextMock = new Mock<ConsumeContext<PaymentStatusChangedIntegrationEvent>>();

            _consumer = new PaymentStatusChangedEventConsumer(
                OrderRepositoryMock.Object,
                UnitOfWorkMock.Object,
                loggerMock.Object
            );
        }

        [Fact]
        public async Task Consume_Should_UpdateOrderStatus_When_PaymentSucceeded()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = Order.Create(orderId, Guid.NewGuid(), "test@example.com", "+7 (999) 123-45-67", new Address("Адрес", 0, 0, "1"));

            OrderRepositoryMock
                .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var message = new PaymentStatusChangedIntegrationEvent(orderId, PaymentStatus.Succeeded);
            _consumeContextMock.Setup(x => x.Message).Returns(message);

            // Act
            await _consumer.Consume(_consumeContextMock.Object);

            // Assert
            order.Status.Should().Be(OrderStatus.Paid);
            UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
