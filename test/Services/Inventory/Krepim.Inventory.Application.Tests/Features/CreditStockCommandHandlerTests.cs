using FluentAssertions;
using Krepim.Inventory.Application.Features.CreditStock;
using Krepim.Inventory.Domain.Entities;
using Moq;

namespace Krepim.Inventory.Application.Tests.Features
{
    public class CreditStockCommandHandlerTests : InventoryApplicationTestBase
    {
        private readonly CreditStockCommandHandler _handler;

        public CreditStockCommandHandlerTests()
        {
            _handler = new CreditStockCommandHandler(RepositoryMock.Object, UnitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_Should_CreateNewStockItem_When_ProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new CreditStockCommand(productId, 100);

            RepositoryMock
                .Setup(x => x.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((StockItem?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            RepositoryMock.Verify(x => x.AddAsync(It.Is<StockItem>(s => s.ProductId == productId), It.IsAny<CancellationToken>()), Times.Once);
            UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_CreditExistingStockItem_When_ProductExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var stockItem = StockItem.Create(productId, 50).Value;
            var command = new CreditStockCommand(productId, 50);

            RepositoryMock
                .Setup(x => x.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(stockItem);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            stockItem.TotalQuantity.Should().Be(100); // 50 + 50
            UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_DomainValidationFails()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var stockItem = StockItem.Create(productId, 50).Value;
            var command = new CreditStockCommand(productId, -10);

            RepositoryMock
                .Setup(x => x.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(stockItem);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Inventory.InvalidOperation");
            UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
