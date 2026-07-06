using FluentAssertions;
using Krepim.Basket.Domain.Entities;

namespace Krepim.Basket.Domain.Tests.Entities
{
    public class BasketItemTests
    {
        [Fact]
        public void Constructor_Should_CreateItem_WhenQuantityIsValid()
        {
            // Act
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Assert
            item.Quantity.Should().Be(2);
            item.UnitPrice.Should().Be(1000m);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Constructor_Should_ThrowArgumentException_WhenQuantityIsInvalid(int invalidQuantity)
        {
            // Act
            var action = () => new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, invalidQuantity);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AddQuantity_Should_IncreaseQuantity()
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act
            item.AddQuantity(3);

            // Assert
            item.Quantity.Should().Be(5);
        }

        [Fact]
        public void UpdateQuantity_Should_ChangeQuantity_WhenQuantityIsValid()
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act
            item.UpdateQuantity(10);

            // Assert
            item.Quantity.Should().Be(10);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void UpdateQuantity_Should_ThrowArgumentException_WhenQuantityIsInvalid(int invalidQuantity)
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act
            var action = () => item.UpdateQuantity(invalidQuantity);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void UpdateQuantityAndPrice_Should_UpdateValues_WhenDataIsValid()
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act
            item.UpdateQuantityAndPrice(5, 1200m);

            // Assert
            item.Quantity.Should().Be(5);
            item.UnitPrice.Should().Be(1200m);
        }

        [Theory]
        [InlineData(0, 100)]
        [InlineData(5, -10)]
        public void UpdateQuantityAndPrice_Should_ThrowArgumentException_WhenDataIsInvalid(int quantity, decimal price)
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act
            var action = () => item.UpdateQuantityAndPrice(quantity, price);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AddQuantity_Should_ThrowArgumentException_WhenAddingZeroOrNegative()
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act & Assert
            item.Invoking(i => i.AddQuantity(0)).Should().Throw<ArgumentException>();
            item.Invoking(i => i.AddQuantity(-1)).Should().Throw<ArgumentException>();
        }
    }
}
