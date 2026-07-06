using FluentAssertions;
using Krepim.Basket.Domain.Entities;

namespace Krepim.Basket.Domain.Tests.Entities
{
    public class CustomerBasketTests
    {
        [Fact]
        public void AddItem_Should_AddNewItem_WhenProductIsNotInBasket()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());
            var item = new BasketItem(Guid.NewGuid(), "Свеча", "SP-1", 500m, 4);

            // Act
            basket.AddItem(item);

            // Assert
            basket.Items.Should().ContainSingle();
            basket.Items[0].Quantity.Should().Be(4);
        }

        [Fact]
        public void AddItem_Should_IncreaseQuantity_WhenProductIsAlreadyInBasket()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());
            var productId = Guid.NewGuid();

            basket.AddItem(new BasketItem(productId, "Свеча", "SP-1", 500m, 2));

            // Act
            basket.AddItem(new BasketItem(productId, "Свеча", "SP-1", 500m, 3));

            // Assert
            basket.Items.Should().ContainSingle("Товар тот же, должна увеличиться только цифра");
            basket.Items[0].Quantity.Should().Be(5);
        }

        [Fact]
        public void TotalPrice_Should_CalculateCorrectly()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());
            basket.AddItem(new BasketItem(Guid.NewGuid(), "Свеча", "SP-1", 500m, 2)); // 1000
            basket.AddItem(new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1500m, 1)); // 1500

            // Act & Assert
            basket.TotalPrice.Should().Be(2500m);
        }

        [Fact]
        public void RemoveItem_Should_RemoveOnlySpecifiedProduct()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();

            basket.AddItem(new BasketItem(productId1, "Свеча", "SP-1", 500m, 2));
            basket.AddItem(new BasketItem(productId2, "Масло", "OIL-1", 1500m, 1));

            // Act
            basket.RemoveItem(productId1);

            // Assert
            basket.Items.Should().ContainSingle();
            basket.Items[0].ProductId.Should().Be(productId2);
        }

        [Fact]
        public void Clear_Should_RemoveAllItems()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());
            basket.AddItem(new BasketItem(Guid.NewGuid(), "Свеча", "SP-1", 500m, 2));
            basket.AddItem(new BasketItem(Guid.NewGuid(), "Масло", "OIL-1", 1500m, 1));

            // Act
            basket.Clear();

            // Assert
            basket.Items.Should().BeEmpty();
            basket.TotalPrice.Should().Be(0);
        }

        [Fact]
        public void RemoveItem_Should_DoNothing_WhenProductIdDoesNotExist()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());
            var productId = Guid.NewGuid();
            basket.AddItem(new BasketItem(productId, "Свеча", "SP-1", 500m, 2));

            // Act
            var nonExistentId = Guid.NewGuid();
            basket.RemoveItem(nonExistentId);

            // Assert
            basket.Items.Should().ContainSingle();
            basket.Items[0].ProductId.Should().Be(productId);
        }

        [Fact]
        public void TotalPrice_Should_BeZero_WhenBasketIsEmpty()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());

            // Act & Assert
            basket.TotalPrice.Should().Be(0);
        }
    }
}
