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

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void CreditStock_Should_ReturnFailure_When_QuantityIsInvalid(int invalidQuantity)
        {
            var stock = StockItem.Create(_productId, 100).Value;
            var result = stock.CreditStock(invalidQuantity);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Inventory.InvalidOperation");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ReserveStock_Should_ReturnFailure_When_QuantityIsInvalid(int invalidQuantity)
        {
            var stock = StockItem.Create(_productId, 100).Value;
            var result = stock.ReserveStock(invalidQuantity);

            result.IsFailure.Should().BeTrue();
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
        public void ConfirmReservation_Should_ReturnFailure_WhenConfirmingMoreThanReserved()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;
            stock.ReserveStock(10);

            // Act
            var result = stock.ConfirmReservation(20);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Inventory.InvalidReservationConfirm");

            stock.TotalQuantity.Should().Be(100);
            stock.ReservedQuantity.Should().Be(10);
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

        [Fact]
        public void ConfirmReservation_Should_HandlePartialConfirmation()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;
            stock.ReserveStock(50);

            // Act
            var result = stock.ConfirmReservation(30);

            // Assert
            result.IsSuccess.Should().BeTrue();
            stock.TotalQuantity.Should().Be(70);
            stock.ReservedQuantity.Should().Be(20);
            stock.AvailableQuantity.Should().Be(50);
        }

        [Fact]
        public void Create_Should_GenerateNewId_When_Valid()
        {
            // Act
            var result1 = StockItem.Create(_productId, 100);
            var result2 = StockItem.Create(_productId, 200);

            // Assert
            result1.Value.Id.Should().NotBe(result2.Value.Id);
        }

        [Fact]
        public void CreditStock_Should_IncreaseTotalQuantity_When_Valid()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;

            // Act
            var result = stock.CreditStock(50);

            // Assert
            result.IsSuccess.Should().BeTrue();
            stock.TotalQuantity.Should().Be(150);
            stock.AvailableQuantity.Should().Be(150);
        }

        [Fact]
        public void CreditStock_Should_ReturnFailure_When_QuantityIsZero()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;

            // Act
            var result = stock.CreditStock(0);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Inventory.InvalidOperation");
            stock.TotalQuantity.Should().Be(100);
        }

        [Fact]
        public void ReserveStock_Should_ReturnFailure_When_QuantityExceedsAvailable()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 50).Value;

            // Act
            var result = stock.ReserveStock(100);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Inventory.InsufficientStock");
            result.Error.Description.Should().Contain("Запрошено: 100, Доступно: 50");
        }

        [Fact]
        public void ReserveStock_Should_ReturnFailure_When_QuantityIsZero()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;

            // Act
            var result = stock.ReserveStock(0);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Inventory.InvalidOperation");
        }

        [Fact]
        public void ConfirmReservation_Should_ReturnFailure_When_QuantityIsZero()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;
            stock.ReserveStock(50);

            // Act
            var result = stock.ConfirmReservation(0);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Inventory.InvalidOperation");
            stock.ReservedQuantity.Should().Be(50);
            stock.TotalQuantity.Should().Be(100);
        }

        [Fact]
        public void ConfirmReservation_Should_ReturnFailure_When_QuantityExceedsReserved()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;
            stock.ReserveStock(30);

            // Act
            var result = stock.ConfirmReservation(40);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Inventory.InvalidReservationConfirm");
            result.Error.Description.Should().Contain("Запрошено: 40, В резерве: 30");
        }

        [Fact]
        public void CancelReservation_Should_ReturnFailure_When_QuantityIsZero()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;
            stock.ReserveStock(50);

            // Act
            var result = stock.CancelReservation(0);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Inventory.InvalidOperation");
            stock.ReservedQuantity.Should().Be(50);
        }

        [Fact]
        public void CancelReservation_Should_ReturnFailure_When_QuantityExceedsReserved()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;
            stock.ReserveStock(20);

            // Act
            var result = stock.CancelReservation(30);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Inventory.InvalidReservationCancel");
            result.Error.Description.Should().Contain("Запрошено: 30, В резерве: 20");
        }

        [Fact]
        public void MultipleOperations_Should_MaintainCorrectState()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;

            // Act
            stock.CreditStock(50);
            stock.ReserveStock(30);
            stock.ConfirmReservation(20);
            stock.CancelReservation(10);

            // Assert
            stock.TotalQuantity.Should().Be(130);
            stock.ReservedQuantity.Should().Be(0);
            stock.AvailableQuantity.Should().Be(130);
        }

        [Fact]
        public void ReserveStock_Should_NotAllowReservingMoreThanAvailable_AfterPartialConfirmation()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;
            stock.ReserveStock(80);
            stock.ConfirmReservation(50);

            // Act
            var result = stock.ReserveStock(30);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Inventory.InsufficientStock");
            stock.AvailableQuantity.Should().Be(20);
        }

        [Fact]
        public void CreditStock_Should_Work_When_ReservedQuantityExists()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;
            stock.ReserveStock(40);

            // Act
            var result = stock.CreditStock(30);

            // Assert
            result.IsSuccess.Should().BeTrue();
            stock.TotalQuantity.Should().Be(130);
            stock.ReservedQuantity.Should().Be(40);
            stock.AvailableQuantity.Should().Be(90);
        }

        [Fact]
        public void ConfirmReservation_Should_NotChangeReservedQuantity_When_Failure()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;
            stock.ReserveStock(30);

            // Act
            var result = stock.ConfirmReservation(50);

            // Assert
            result.IsFailure.Should().BeTrue();
            stock.ReservedQuantity.Should().Be(30);
            stock.TotalQuantity.Should().Be(100);
            stock.AvailableQuantity.Should().Be(70);
        }

        [Fact]
        public void CancelReservation_Should_NotChangeTotalQuantity_When_Failure()
        {
            // Arrange
            var stock = StockItem.Create(_productId, 100).Value;
            stock.ReserveStock(20);

            // Act
            var result = stock.CancelReservation(30);

            // Assert
            result.IsFailure.Should().BeTrue();
            stock.TotalQuantity.Should().Be(100);
            stock.ReservedQuantity.Should().Be(20);
            stock.AvailableQuantity.Should().Be(80);
        }
    }
}
