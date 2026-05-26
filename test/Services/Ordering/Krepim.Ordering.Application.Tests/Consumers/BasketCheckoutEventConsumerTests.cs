using FluentAssertions;
using Krepim.EventBus.Events.Basket;
using Krepim.Ordering.Application.Consumers;
using Krepim.Ordering.Domain.Entities;
using Krepim.Ordering.Domain.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Krepim.Ordering.Application.Tests.Consumers
{
    public class BasketCheckoutEventConsumerTests : OrderingApplicationTestBase
    {
        private readonly BasketCheckoutEventConsumer _consumer;
        private readonly Mock<ILogger<BasketCheckoutEventConsumer>> _loggerMock;
        private readonly Mock<ConsumeContext<BasketCheckoutIntegrationEvent>> _consumeContextMock;

        public BasketCheckoutEventConsumerTests()
        {
            _loggerMock = new Mock<ILogger<BasketCheckoutEventConsumer>>();
            _consumeContextMock = new Mock<ConsumeContext<BasketCheckoutIntegrationEvent>>();

            _consumer = new BasketCheckoutEventConsumer(
                OrderRepositoryMock.Object,
                UnitOfWorkMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Consume_Should_CreateAndSaveOrder_When_CheckoutEventIsReceived()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();

            var checkoutItems = new List<BasketCheckoutItem>
            {
                new(productId1, 150.00m, 2),
                new(productId2, 50.00m, 5)
            };

            var message = new BasketCheckoutIntegrationEvent(
                userId,
                550.00m,
                "Москва",
                "Русаковская",
                "101000",
                checkoutItems);

            _consumeContextMock.Setup(x => x.Message).Returns(message);

            Order? savedOrder = null;
            OrderRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .Callback<Order, CancellationToken>((order, _) => savedOrder = order)
                .Returns(Task.CompletedTask);

            // Act
            await _consumer.Consume(_consumeContextMock.Object);

            // Assert
            savedOrder.Should().NotBeNull();
            savedOrder!.UserId.Should().Be(userId);
            savedOrder.Status.Should().Be(OrderStatus.Pending);
            savedOrder.TotalPrice.Should().Be(550.00m);
            savedOrder.Items.Should().HaveCount(2);
            savedOrder.ShippingAddress.City.Should().Be("Москва");
            savedOrder.ShippingAddress.Street.Should().Be("Русаковская");
            savedOrder.ShippingAddress.ZipCode.Should().Be("101000");

            UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
