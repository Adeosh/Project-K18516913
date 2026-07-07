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

        [Fact]
        public void Constructor_Should_SetAllProperties_WhenValid()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var productName = "Масло моторное";
            var sku = "OIL-001";
            var unitPrice = 1500m;
            var quantity = 3;

            // Act
            var item = new BasketItem(productId, productName, sku, unitPrice, quantity);

            // Assert
            item.ProductId.Should().Be(productId);
            item.ProductName.Should().Be(productName);
            item.Sku.Should().Be(sku);
            item.UnitPrice.Should().Be(unitPrice);
            item.Quantity.Should().Be(quantity);
        }

        [Fact]
        public void Constructor_Should_ThrowArgumentException_When_QuantityIsZero()
        {
            // Act
            var action = () => new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 0);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithParameterName("quantity")
                .WithMessage("Количество должно быть больше нуля*");
        }

        [Fact]
        public void Constructor_Should_ThrowArgumentException_When_QuantityIsNegative()
        {
            // Act
            var action = () => new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, -5);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithParameterName("quantity")
                .WithMessage("Количество должно быть больше нуля*");
        }

        [Fact]
        public void AddQuantity_Should_IncreaseQuantity_When_Positive()
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act
            item.AddQuantity(3);

            // Assert
            item.Quantity.Should().Be(5);
        }

        [Fact]
        public void AddQuantity_Should_ThrowArgumentException_When_QuantityIsZero()
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act
            var action = () => item.AddQuantity(0);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithParameterName("quantity")
                .WithMessage("Добавляемое количество должно быть больше нуля*");
        }

        [Fact]
        public void AddQuantity_Should_ThrowArgumentException_When_QuantityIsNegative()
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act
            var action = () => item.AddQuantity(-3);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithParameterName("quantity")
                .WithMessage("Добавляемое количество должно быть больше нуля*");
        }

        [Fact]
        public void UpdateQuantity_Should_ChangeQuantity_When_Valid()
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act
            item.UpdateQuantity(10);

            // Assert
            item.Quantity.Should().Be(10);
        }

        [Fact]
        public void UpdateQuantity_Should_ThrowArgumentException_When_NewQuantityIsZero()
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act
            var action = () => item.UpdateQuantity(0);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithParameterName("newQuantity")
                .WithMessage("Новое значение должно быть больше нуля*");
        }

        [Fact]
        public void UpdateQuantity_Should_ThrowArgumentException_When_NewQuantityIsNegative()
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act
            var action = () => item.UpdateQuantity(-5);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithParameterName("newQuantity")
                .WithMessage("Новое значение должно быть больше нуля*");
        }

        [Fact]
        public void UpdateQuantityAndPrice_Should_UpdateBoth_When_Valid()
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act
            item.UpdateQuantityAndPrice(5, 1200m);

            // Assert
            item.Quantity.Should().Be(5);
            item.UnitPrice.Should().Be(1200m);
        }

        [Fact]
        public void UpdateQuantityAndPrice_Should_ThrowArgumentException_When_QuantityIsZero()
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act
            var action = () => item.UpdateQuantityAndPrice(0, 1000m);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithParameterName("newQuantity")
                .WithMessage("Количество должно быть больше нуля*");
        }

        [Fact]
        public void UpdateQuantityAndPrice_Should_ThrowArgumentException_When_QuantityIsNegative()
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act
            var action = () => item.UpdateQuantityAndPrice(-5, 1000m);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithParameterName("newQuantity")
                .WithMessage("Количество должно быть больше нуля*");
        }

        [Fact]
        public void UpdateQuantityAndPrice_Should_ThrowArgumentException_When_PriceIsNegative()
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act
            var action = () => item.UpdateQuantityAndPrice(5, -100m);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithParameterName("newPrice")
                .WithMessage("Цена не может быть отрицательной*");
        }

        [Fact]
        public void UpdateQuantityAndPrice_Should_AllowZeroPrice_When_Valid()
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 2);

            // Act
            item.UpdateQuantityAndPrice(5, 0m);

            // Assert
            item.Quantity.Should().Be(5);
            item.UnitPrice.Should().Be(0m);
        }

        [Fact]
        public void Constructor_Should_AllowZeroPrice_When_QuantityValid()
        {
            // Act
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 0m, 2);

            // Assert
            item.UnitPrice.Should().Be(0m);
            item.Quantity.Should().Be(2);
        }

        [Fact]
        public void Constructor_Should_AllowDecimalPrice_When_Valid()
        {
            // Act
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 999.99m, 2);

            // Assert
            item.UnitPrice.Should().Be(999.99m);
        }

        [Fact]
        public void AddQuantity_Should_WorkWithMultipleCalls()
        {
            // Arrange
            var item = new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1000m, 1);

            // Act
            item.AddQuantity(2);
            item.AddQuantity(3);
            item.AddQuantity(4);

            // Assert
            item.Quantity.Should().Be(10);
        }
    }
}
