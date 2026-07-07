using Krepim.SharedKernel.Domain;

namespace Krepim.Ordering.Domain.Entities
{
    public sealed class OrderItem : Entity<Guid>
    {
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }

        #region For EF
        private OrderItem() : base(Guid.NewGuid()) { }
        #endregion

        internal OrderItem(Guid orderId, Guid productId, decimal unitPrice, int quantity)
            : base(Guid.NewGuid())
        {
            if (quantity <= 0)
                throw new ArgumentException("Количество должно быть положительным", nameof(quantity));

            OrderId = orderId;
            ProductId = productId;
            UnitPrice = unitPrice;
            Quantity = quantity;
        }
    }
}
