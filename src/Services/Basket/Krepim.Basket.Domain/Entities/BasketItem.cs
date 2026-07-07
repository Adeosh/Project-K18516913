using System.Text.Json.Serialization;

namespace Krepim.Basket.Domain.Entities
{
    public sealed class BasketItem
    {
        [JsonInclude]
        public Guid ProductId { get; private set; }

        [JsonInclude]
        public string ProductName { get; private set; }

        [JsonInclude]
        public string Sku { get; private set; }

        [JsonInclude]
        public decimal UnitPrice { get; private set; }

        [JsonInclude]
        public int Quantity { get; private set; }

        #region For Redis
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public BasketItem() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        #endregion

        public BasketItem(Guid productId, string productName, string sku, decimal unitPrice, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Количество должно быть больше нуля", nameof(quantity));

            ProductId = productId;
            ProductName = productName;
            Sku = sku;
            UnitPrice = unitPrice;
            Quantity = quantity;
        }

        public void AddQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Добавляемое количество должно быть больше нуля", nameof(quantity));

            Quantity += quantity;
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Новое значение должно быть больше нуля", nameof(newQuantity));

            Quantity = newQuantity;
        }

        public void UpdateQuantityAndPrice(int newQuantity, decimal newPrice)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Количество должно быть больше нуля", nameof(newQuantity));

            if (newPrice < 0)
                throw new ArgumentException("Цена не может быть отрицательной", nameof(newPrice));

            Quantity = newQuantity;
            UnitPrice = newPrice;
        }
    }
}
