namespace Krepim.Mobile.Features.Basket.Models.Exchange
{
    public record AddBasketItemRequest(
        string ProductId,
        string ProductName,
        string Sku,
        decimal UnitPrice,
        int Quantity
    );
}
