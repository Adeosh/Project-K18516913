using System.Text.Json.Serialization;

namespace Krepim.Basket.Domain.Entities
{
    public sealed class CustomerBasket
    {
        [JsonInclude]
        public Guid UserId { get; private set; }

        [JsonInclude]
        public List<BasketItem> Items { get; private set; } = new();

        public decimal TotalPrice => Items.Sum(x => x.UnitPrice * x.Quantity);

        #region For EF
        public CustomerBasket() { }
        #endregion

        public CustomerBasket(Guid userId)
        {
            UserId = userId;
        }

        public void AddItem(BasketItem item)
        {
            BasketItem? existingItem = Items.FirstOrDefault(x => x.ProductId == item.ProductId);

            if (existingItem is not null)
                existingItem.AddQuantity(item.Quantity);
            else
                Items.Add(item);
        }

        public void RemoveItem(Guid productId)
        {
            Items.RemoveAll(x => x.ProductId == productId);
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}
