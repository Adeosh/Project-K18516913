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
    }
}
