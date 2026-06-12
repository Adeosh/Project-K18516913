namespace Krepim.Basket.Application.Models.Exchange
{
    public record AddItemRequest(Guid ProductId, string ProductName, string Sku, decimal UnitPrice, int Quantity);

}
