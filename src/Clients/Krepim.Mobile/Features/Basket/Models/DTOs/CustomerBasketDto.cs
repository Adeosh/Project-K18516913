namespace Krepim.Mobile.Features.Basket.Models.DTOs
{
    public class CustomerBasketDto
    {
        public string BuyerId { get; set; } = string.Empty;
        public List<BasketItemDto> Items { get; set; } = new();
    }
}
