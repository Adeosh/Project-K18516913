namespace Krepim.Basket.Application.Models
{
    public record AddItemRequest(Guid ProductId, string ProductName, string Sku, decimal UnitPrice, int Quantity);

}
