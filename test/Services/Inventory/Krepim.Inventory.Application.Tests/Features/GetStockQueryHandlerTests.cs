using FluentAssertions;
using Krepim.Inventory.Application.Features.GetStock;
using Krepim.Inventory.Domain.Entities;
using Moq;

namespace Krepim.Inventory.Application.Tests.Features
{
    public class GetStockQueryHandlerTests : InventoryApplicationTestBase
    {
        private readonly GetStockQueryHandler _handler;

        public GetStockQueryHandlerTests()
        {
            _handler = new GetStockQueryHandler(RepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccessWithCorrectQuantity_When_ProductExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var stockItem = StockItem.Create(productId, 45).Value;

            RepositoryMock
                .Setup(x => x.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(stockItem);

            var query = new GetStockQuery(productId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.ProductId.Should().Be(productId);
            result.Value.AvailableQuantity.Should().Be(45);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailureError_When_ProductNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            RepositoryMock
                .Setup(x => x.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((StockItem?)null);

            var query = new GetStockQuery(productId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(SharedKernel.Results.ErrorType.NotFound);
        }
    }
}
