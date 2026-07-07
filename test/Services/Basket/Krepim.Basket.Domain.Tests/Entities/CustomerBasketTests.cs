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

        [Fact]
        public void Constructor_Should_InitializeWithUserId_When_Valid()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var basket = new CustomerBasket(userId);

            // Assert
            basket.UserId.Should().Be(userId);
            basket.Items.Should().BeEmpty();
            basket.TotalPrice.Should().Be(0);
        }

        [Fact]
        public void Constructor_Should_InitializeEmptyItems_When_Default()
        {
            // Act
            var basket = new CustomerBasket();

            // Assert
            basket.UserId.Should().Be(Guid.Empty);
            basket.Items.Should().BeEmpty();
        }

        [Fact]
        public void AddItem_Should_AddNewItem_When_ProductNotInBasket()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());
            var item = new BasketItem(Guid.NewGuid(), "Свеча", "SP-1", 500m, 4);

            // Act
            basket.AddItem(item);

            // Assert
            basket.Items.Should().ContainSingle();
            basket.Items.First().Should().Be(item);
        }

        [Fact]
        public void AddItem_Should_IncreaseQuantity_When_ProductAlreadyExists()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());
            var productId = Guid.NewGuid();
            basket.AddItem(new BasketItem(productId, "Свеча", "SP-1", 500m, 2));

            // Act
            basket.AddItem(new BasketItem(productId, "Свеча", "SP-1", 500m, 3));

            // Assert
            basket.Items.Should().ContainSingle();
            basket.Items[0].Quantity.Should().Be(5);
        }

        [Fact]
        public void AddItem_Should_NotChangeOtherItems_When_AddingExistingProduct()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            basket.AddItem(new BasketItem(productId1, "Свеча", "SP-1", 500m, 2));
            basket.AddItem(new BasketItem(productId2, "Масло", "OIL-1", 1500m, 1));

            // Act
            basket.AddItem(new BasketItem(productId1, "Свеча", "SP-1", 500m, 3));

            // Assert
            basket.Items.Should().HaveCount(2);
            basket.Items.First(x => x.ProductId == productId1).Quantity.Should().Be(5);
            basket.Items.First(x => x.ProductId == productId2).Quantity.Should().Be(1);
        }

        [Fact]
        public void AddItem_Should_AddItemWithZeroPrice_When_Valid()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());
            var item = new BasketItem(Guid.NewGuid(), "Бесплатный товар", "FREE-1", 0m, 1);

            // Act
            basket.AddItem(item);

            // Assert
            basket.Items.Should().ContainSingle();
            basket.TotalPrice.Should().Be(0);
        }

        [Fact]
        public void AddItem_Should_UseExistingItemPrice_When_AddingDuplicate()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());
            var productId = Guid.NewGuid();
            basket.AddItem(new BasketItem(productId, "Свеча", "SP-1", 500m, 2));

            // Act
            basket.AddItem(new BasketItem(productId, "Свеча", "SP-1", 600m, 3));

            // Assert
            basket.Items.Should().ContainSingle();
            basket.Items[0].Quantity.Should().Be(5);
            basket.Items[0].UnitPrice.Should().Be(500m); // Цена не изменилась
        }

        [Fact]
        public void RemoveItem_Should_RemoveItem_When_ProductExists()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());
            var productId = Guid.NewGuid();
            basket.AddItem(new BasketItem(productId, "Свеча", "SP-1", 500m, 2));

            // Act
            basket.RemoveItem(productId);

            // Assert
            basket.Items.Should().BeEmpty();
            basket.TotalPrice.Should().Be(0);
        }

        [Fact]
        public void RemoveItem_Should_RemoveOnlyMatchingProduct_When_MultipleItems()
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
            basket.Items.First().ProductId.Should().Be(productId2);
            basket.TotalPrice.Should().Be(1500m);
        }

        [Fact]
        public void RemoveItem_Should_DoNothing_When_ProductNotExists()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());
            var productId = Guid.NewGuid();
            basket.AddItem(new BasketItem(productId, "Свеча", "SP-1", 500m, 2));

            // Act
            basket.RemoveItem(Guid.NewGuid());

            // Assert
            basket.Items.Should().ContainSingle();
            basket.Items.First().ProductId.Should().Be(productId);
        }

        [Fact]
        public void RemoveItem_Should_DoNothing_When_BasketIsEmpty()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());

            // Act
            basket.RemoveItem(Guid.NewGuid());

            // Assert
            basket.Items.Should().BeEmpty();
            basket.TotalPrice.Should().Be(0);
        }

        [Fact]
        public void Clear_Should_RemoveAllItems_When_ItemsExist()
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
        public void Clear_Should_DoNothing_When_BasketIsEmpty()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());

            // Act
            basket.Clear();

            // Assert
            basket.Items.Should().BeEmpty();
            basket.TotalPrice.Should().Be(0);
        }

        [Fact]
        public void TotalPrice_Should_Recalculate_AfterMultipleOperations()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());
            var productId = Guid.NewGuid();
            basket.AddItem(new BasketItem(productId, "Свеча", "SP-1", 500m, 2));

            // Act
            basket.AddItem(new BasketItem(productId, "Свеча", "SP-1", 500m, 3));
            basket.RemoveItem(productId);

            // Assert
            basket.TotalPrice.Should().Be(0);
        }

        [Fact]
        public void TotalPrice_Should_Recalculate_WhenAddingMultipleItems()
        {
            // Arrange
            var basket = new CustomerBasket(Guid.NewGuid());

            // Act
            basket.AddItem(new BasketItem(Guid.NewGuid(), "Товар 1", "SKU-1", 100m, 2));
            basket.AddItem(new BasketItem(Guid.NewGuid(), "Товар 2", "SKU-2", 200m, 3));
            basket.AddItem(new BasketItem(Guid.NewGuid(), "Товар 3", "SKU-3", 50m, 5));

            // Assert
            basket.TotalPrice.Should().Be(100m * 2 + 200m * 3 + 50m * 5);
            basket.TotalPrice.Should().Be(1050m);
        }
    }
}
