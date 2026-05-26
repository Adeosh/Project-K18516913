using FluentAssertions;
using Krepim.EventBus.Events.Inventory;
using Krepim.Inventory.Application.Consumers;
using Krepim.Inventory.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Krepim.Inventory.Application.Tests.Consumers
{
    public class OrderCreatedEventConsumerTests : InventoryApplicationTestBase
    {
        private readonly OrderCreatedEventConsumer _consumer;
        private readonly Mock<ILogger<OrderCreatedEventConsumer>> _loggerMock;
        private readonly Mock<ConsumeContext<OrderCreatedIntegrationEvent>> _consumeContextMock;

        public OrderCreatedEventConsumerTests()
        {
            _loggerMock = new Mock<ILogger<OrderCreatedEventConsumer>>();
            _consumeContextMock = new Mock<ConsumeContext<OrderCreatedIntegrationEvent>>();

            _consumer = new OrderCreatedEventConsumer(
                RepositoryMock.Object,
                UnitOfWorkMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Consume_Should_ReserveStock_When_ProductExistsAndQuantityIsAvailable()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var stockItem = StockItem.Create(productId, 100).Value;

            var message = new OrderCreatedIntegrationEvent(
                Guid.NewGuid(),
                1500m,
                new List<OrderItemPayload> { new(productId, 30) });

            _consumeContextMock.Setup(x => x.Message).Returns(message);

            RepositoryMock
                .Setup(x => x.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(stockItem);

            // Act
            await _consumer.Consume(_consumeContextMock.Object);

            // Assert
            stockItem.AvailableQuantity.Should().Be(70);
            UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Consume_Should_ThrowException_When_ProductDoesNotExistOnStock()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var message = new OrderCreatedIntegrationEvent(
                Guid.NewGuid(),
                500m,
                new List<OrderItemPayload> { new(productId, 5) });

            _consumeContextMock.Setup(x => x.Message).Returns(message);

            RepositoryMock
                .Setup(x => x.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((StockItem?)null);

            // Act
            Func<Task> act = async () => await _consumer.Consume(_consumeContextMock.Object);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Товар {productId} не найден на складе.");

            UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
