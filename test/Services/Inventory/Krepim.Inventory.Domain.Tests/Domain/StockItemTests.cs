using FluentAssertions;
using Krepim.Inventory.Domain.Entities;
using Krepim.SharedKernel.Results;

namespace Krepim.Inventory.Domain.Tests.Domain
{
    public class StockItemTests
    {
        private readonly Guid _productId = Guid.NewGuid();

        [Fact]
        public void Create_Should_SetInitialState_WhenQuantityIsValid()
        {
            // Act
            var result = StockItem.Create(_productId, 100);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.TotalQuantity.Should().Be(100);
            result.Value.ReservedQuantity.Should().Be(0);
            result.Value.AvailableQuantity.Should().Be(100);
        }

        [Fact]
        public void Create_Should_ReturnFailure_WhenQuantityIsNegative()
        {
            // Act
            var result = StockItem.Create(_productId, -5);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public void ReserveStock_Should_MoveAvailableToReserved_WhenSufficient()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;

            // Act
            var result = stock.ReserveStock(30);

            // Assert
            result.IsSuccess.Should().BeTrue();
            stock.ReservedQuantity.Should().Be(30);
            stock.AvailableQuantity.Should().Be(70);
            stock.TotalQuantity.Should().Be(100);
        }

        [Fact]
        public void ReserveStock_Should_ReturnFailure_WhenInsufficient()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 20).Value;

            // Act
            var result = stock.ReserveStock(30);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Inventory.InsufficientStock");

            // Инварианты не должны измениться
            stock.ReservedQuantity.Should().Be(0);
            stock.AvailableQuantity.Should().Be(20);
        }

        [Fact]
        public void ConfirmReservation_Should_DecreaseTotalAndReserved()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;
            stock.ReserveStock(40);

            // Act - товар уезжает со склада
            var result = stock.ConfirmReservation(40);

            // Assert
            result.IsSuccess.Should().BeTrue();
            stock.TotalQuantity.Should().Be(60);
            stock.ReservedQuantity.Should().Be(0);
            stock.AvailableQuantity.Should().Be(60);
        }

        [Fact]
        public void CancelReservation_Should_DecreaseReservedAndRestoreAvailable()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;
            stock.ReserveStock(30);

            // Act
            var result = stock.CancelReservation(30);

            // Assert
            result.IsSuccess.Should().BeTrue();
            stock.TotalQuantity.Should().Be(100);
            stock.ReservedQuantity.Should().Be(0);
            stock.AvailableQuantity.Should().Be(100);
        }

        [Fact]
        public void CancelReservation_Should_ReturnFailure_WhenCancellingMoreThanReserved()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;
            stock.ReserveStock(10);

            // Act
            var result = stock.CancelReservation(20);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Inventory.InvalidReservationCancel");
        }
    }
}
