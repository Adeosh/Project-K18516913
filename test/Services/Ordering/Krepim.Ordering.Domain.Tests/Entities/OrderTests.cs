using FluentAssertions;
using Krepim.Ordering.Domain.Entities;
using Krepim.SharedKernel.Enums;
using Krepim.SharedKernel.ValueObjects;


namespace Krepim.Ordering.Domain.Tests.Entities
{
    public class OrderTests
    {
        private readonly Address _testAddress = new("Москва, ул.Русаковская", 10000.00, 10550.00, "12");

        [Fact]
        public void Create_Should_InitializeOrderWithCorrectDefaults()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            // Act
            var order = Order.Create(orderId, userId, _testAddress);

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
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);
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
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);

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
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);
            order.MarkAsPaid();

            // Act
            Action act = () => order.AddOrderItem(Guid.NewGuid(), 100m, 1);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Можно добавлять товары только в отложенные заказы.");
        }

        [Fact]
        public void MarkAsPaid_Should_ChangeStatusToPaid_When_OrderIsPending()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);

            // Act
            order.MarkAsPaid();

            // Assert
            order.Status.Should().Be(OrderStatus.Paid);
        }

        [Fact]
        public void MarkAsPaid_Should_ThrowInvalidOperationException_When_OrderIsCancelled()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);
            order.Cancel();

            // Act
            Action act = () => order.MarkAsPaid();

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Cancel_Should_ChangeStatusToCancelled_When_OrderIsPending()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);

            // Act
            order.Cancel();

            // Assert
            order.Status.Should().Be(OrderStatus.Cancelled);
        }

        [Fact]
        public void HandlePaymentResult_Should_SetStatusToAwaitingValidation_When_StatusIsPending_And_PaymentIsPending()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);

            // Act
            order.HandlePaymentResult(PaymentStatus.Pending);

            // Assert
            order.Status.Should().Be(OrderStatus.AwaitingValidation);
        }

        [Fact]
        public void HandlePaymentResult_Should_MarkAsPaid_When_PaymentSucceeded()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);

            // Act
            order.HandlePaymentResult(PaymentStatus.Succeeded);

            // Assert
            order.Status.Should().Be(OrderStatus.Paid);
        }

        [Fact]
        public void HandlePaymentResult_Should_CancelOrder_When_PaymentFailed()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);

            // Act
            order.HandlePaymentResult(PaymentStatus.Failed);

            // Assert
            order.Status.Should().Be(OrderStatus.Cancelled);
        }

        [Fact]
        public void HandlePaymentResult_Should_CancelOrder_When_StatusIsPaid_And_Refunded()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);
            order.MarkAsPaid();

            // Act
            order.HandlePaymentResult(PaymentStatus.Refunded);

            // Assert
            order.Status.Should().Be(OrderStatus.Cancelled);
        }

        [Fact]
        public void HandlePaymentResult_Should_NotChangeStatus_When_OrderIsCancelledOrShipped()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);
            order.Cancel();

            // Act
            order.HandlePaymentResult(PaymentStatus.Succeeded);

            // Assert
            order.Status.Should().Be(OrderStatus.Cancelled);
        }

        [Fact]
        public void AddOrderItem_Should_NotThrowException_When_UnitPriceIsZeroOrNegative()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);

            // Act
            Action actZero = () => order.AddOrderItem(Guid.NewGuid(), 0m, 1);
            Action actNegative = () => order.AddOrderItem(Guid.NewGuid(), -100m, 1);

            // Assert
            actZero.Should().NotThrow();
            actNegative.Should().NotThrow();

            order.Items.Should().HaveCount(2);
            order.Items.Should().Contain(x => x.UnitPrice == 0m);
            order.Items.Should().Contain(x => x.UnitPrice == -100m);
        }

        [Fact]
        public void Cancel_Should_ThrowInvalidOperationException_When_OrderIsAlreadyPaidOrShipped()
        {
            // Arrange
            var orderPaid = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);
            orderPaid.MarkAsPaid();

            // Act
            Action act = () => orderPaid.Cancel();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Невозможно отменить заказ, который уже оплачен или отправлен.");
        }

        [Fact]
        public void Cancel_Should_ThrowInvalidOperationException_When_OrderIsShipped()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);
            // Устанавливаем статус Shipped через рефлексию
            typeof(Order).GetProperty("Status")?.SetValue(order, OrderStatus.Shipped);

            // Act
            Action act = () => order.Cancel();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Невозможно отменить заказ, который уже оплачен или отправлен.");
        }

        [Fact]
        public void MarkAsPaid_Should_ThrowInvalidOperationException_When_OrderIsShipped()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);
            typeof(Order).GetProperty("Status")?.SetValue(order, OrderStatus.Shipped);

            // Act
            Action act = () => order.MarkAsPaid();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Нельзя оплатить заказ, который уже был отправлен.");
        }

        [Fact]
        public void HandlePaymentResult_Should_ThrowArgumentOutOfRangeException_When_UnknownStatus()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);

            // Act
            Action act = () => order.HandlePaymentResult((PaymentStatus)999);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("paymentStatus")
                .WithMessage("*Неизвестный статус оплаты*");
        }

        [Fact]
        public void AddOrderItem_Should_ThrowInvalidOperationException_When_OrderIsAwaitingValidation()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);
            order.HandlePaymentResult(PaymentStatus.Pending);

            // Act
            Action act = () => order.AddOrderItem(Guid.NewGuid(), 100m, 1);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Можно добавлять товары только в отложенные заказы.");
        }

        [Fact]
        public void MarkAsPaid_Should_ThrowInvalidOperationException_When_OrderIsNotPendingOrAwaitingValidation()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);
            order.Cancel();

            // Act
            Action act = () => order.MarkAsPaid();

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Cancel_Should_NotThrow_When_OrderIsPending()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);

            // Act
            Action act = () => order.Cancel();

            // Assert
            act.Should().NotThrow();
            order.Status.Should().Be(OrderStatus.Cancelled);
        }

        [Fact]
        public void Cancel_Should_NotThrow_When_OrderIsAwaitingValidation()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), _testAddress);
            order.HandlePaymentResult(PaymentStatus.Pending);

            // Act
            Action act = () => order.Cancel();

            // Assert
            act.Should().NotThrow();
            order.Status.Should().Be(OrderStatus.Cancelled);
        }
    }
}
