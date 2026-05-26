using FluentAssertions;
using Krepim.Ordering.Domain.Entities;
using Krepim.Ordering.Domain.Enums;
using Krepim.Ordering.Domain.ValueObjects;

namespace Krepim.Ordering.Domain.Tests.Entities
{
    public class OrderTests
    {
        private readonly Address _testAddress = new("Москва", "Русаковская", "101000");

        [Fact]
        public void Create_Should_InitializeOrderWithCorrectDefaults()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var order = Order.Create(userId, _testAddress);

            // Assert
            order.Id.Should().NotBeEmpty();
            order.UserId.Should().Be(userId);
            order.ShippingAddress.Should().Be(_testAddress);
            order.Status.Should().Be(OrderStatus.Pending);
            order.Items.Should().BeEmpty();
            order.TotalPrice.Should().Be(0m);
            order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void AddOrderItem_Should_AddItemAndCalculateTotalPriceCorrectly()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), _testAddress);
            var product1 = Guid.NewGuid();
            var product2 = Guid.NewGuid();

            // Act
            order.AddOrderItem(product1, 150.00m, 2);
            order.AddOrderItem(product2, 50.00m, 5);

            // Assert
            order.Items.Should().HaveCount(2);
            order.TotalPrice.Should().Be(550.00m);

            var firstItem = order.Items.First(x => x.ProductId == product1);
            firstItem.OrderId.Should().Be(order.Id);
            firstItem.UnitPrice.Should().Be(150.00m);
            firstItem.Quantity.Should().Be(2);
        }

        [Fact]
        public void AddOrderItem_Should_ThrowArgumentException_When_QuantityIsZeroOrNegative()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), _testAddress);

            // Act
            Action actZero = () => order.AddOrderItem(Guid.NewGuid(), 100m, 0);
            Action actNegative = () => order.AddOrderItem(Guid.NewGuid(), 100m, -5);

            // Assert
            actZero.Should().Throw<ArgumentException>()
                .WithParameterName("quantity");

            actNegative.Should().Throw<ArgumentException>()
                .WithParameterName("quantity");
        }

        [Fact]
        public void AddOrderItem_Should_ThrowInvalidOperationException_When_OrderStatusIsNotPending()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), _testAddress);
            order.MarkAsPaid();

            // Act
            Action act = () => order.AddOrderItem(Guid.NewGuid(), 100m, 1);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Can only add items to pending orders.");
        }

        [Fact]
        public void MarkAsPaid_Should_ChangeStatusToPaid_When_OrderIsPending()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), _testAddress);

            // Act
            order.MarkAsPaid();

            // Assert
            order.Status.Should().Be(OrderStatus.Paid);
        }

        [Fact]
        public void MarkAsPaid_Should_ThrowInvalidOperationException_When_OrderIsCancelled()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), _testAddress);
            order.Cancel();

            // Act
            Action act = () => order.MarkAsPaid();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot pay for a cancelled order.");
        }

        [Fact]
        public void Cancel_Should_ChangeStatusToCancelled_When_OrderIsPending()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), _testAddress);

            // Act
            order.Cancel();

            // Assert
            order.Status.Should().Be(OrderStatus.Cancelled);
        }

        [Fact]
        public void Cancel_Should_ThrowInvalidOperationException_When_OrderIsAlreadyPaidOrShipped()
        {
            // Arrange
            var orderPaid = Order.Create(Guid.NewGuid(), _testAddress);
            orderPaid.MarkAsPaid();

            // Act
            Action act = () => orderPaid.Cancel();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot cancel an order that is already paid or shipped.");
        }
    }
}
