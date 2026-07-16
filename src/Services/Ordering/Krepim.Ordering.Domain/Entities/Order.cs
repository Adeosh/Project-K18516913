using Krepim.SharedKernel.Domain;
using Krepim.SharedKernel.Enums;
using Krepim.SharedKernel.ValueObjects;

namespace Krepim.Ordering.Domain.Entities
{
    public sealed class Order : AggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
        public string CustomerEmail { get; private set; }
        public string? CustomerPhone { get; private set; }
        public OrderStatus Status { get; private set; }
        public Address ShippingAddress { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private readonly List<OrderItem> _items = new();

        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        public decimal TotalPrice => _items.Sum(x => x.UnitPrice * x.Quantity);

        #region For EF
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private Order() : base(Guid.NewGuid()) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        #endregion

        private Order(Guid id, Guid userId, string customerEmail, string? customerPhone, Address shippingAddress) : base(id)
        {
            UserId = userId;
            CustomerEmail = customerEmail;
            CustomerPhone = customerPhone;
            ShippingAddress = shippingAddress;
            Status = OrderStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public static Order Create(Guid id, Guid userId, string customerEmail, string? customerPhone, Address shippingAddress)
        {
            return new Order(id, userId, customerEmail, customerPhone, shippingAddress);
        }

        public void AddOrderItem(Guid productId, decimal unitPrice, int quantity)
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException("Можно добавлять товары только в отложенные заказы.");

            var item = new OrderItem(Id, productId, unitPrice, quantity);
            _items.Add(item);
        }

        public void MarkAsPaid()
        {
            if (Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Нельзя оплатить отмененный заказ.");

            if (Status == OrderStatus.Shipped)
                throw new InvalidOperationException("Нельзя оплатить заказ, который уже был отправлен.");

            if (Status == OrderStatus.Paid)
                return;

            Status = OrderStatus.Paid;
        }

        public void Cancel()
        {
            if (Status is OrderStatus.Paid or OrderStatus.Shipped)
                throw new InvalidOperationException("Невозможно отменить заказ, который уже оплачен или отправлен.");

            Status = OrderStatus.Cancelled;
        }

        private void SetStatusToCancelled()
        {
            Status = OrderStatus.Cancelled;
        }

        public void HandlePaymentResult(PaymentStatus paymentStatus)
        {
            if (Status is OrderStatus.Cancelled or OrderStatus.Shipped)
                return;

            switch (paymentStatus)
            {
                case PaymentStatus.Pending:
                    if (Status == OrderStatus.Pending)
                        Status = OrderStatus.AwaitingValidation;
                    break;

                case PaymentStatus.Succeeded:
                    if (Status is OrderStatus.Pending or OrderStatus.AwaitingValidation)
                        MarkAsPaid();
                    break;

                case PaymentStatus.Failed:
                    if (Status is OrderStatus.Pending or OrderStatus.AwaitingValidation)
                        Cancel();
                    break;

                case PaymentStatus.Refunded:
                    if (Status == OrderStatus.Paid)
                        SetStatusToCancelled();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(paymentStatus), paymentStatus, "Неизвестный статус оплаты");
            }
        }
    }
}
