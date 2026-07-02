using Krepim.Ordering.Domain.Enums;
using Krepim.SharedKernel.Domain;
using Krepim.SharedKernel.ValueObjects;

namespace Krepim.Ordering.Domain.Entities
{
    public sealed class Order : AggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
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

        private Order(Guid id, Guid userId, Address shippingAddress) : base(id)
        {
            UserId = userId;
            ShippingAddress = shippingAddress;
            Status = OrderStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public static Order Create(Guid id, Guid userId, Address shippingAddress)
        {
            return new Order(id, userId, shippingAddress);
        }

        public void AddOrderItem(Guid productId, decimal unitPrice, int quantity)
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException("Can only add items to pending orders.");

            var item = new OrderItem(Id, productId, unitPrice, quantity);
            _items.Add(item);
        }

        public void MarkAsPaid()
        {
            if (Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Cannot pay for a cancelled order.");

            Status = OrderStatus.Paid;
        }

        public void Cancel()
        {
            if (Status is OrderStatus.Paid or OrderStatus.Shipped)
                throw new InvalidOperationException("Cannot cancel an order that is already paid or shipped.");

            Status = OrderStatus.Cancelled;
        }
    }
}
