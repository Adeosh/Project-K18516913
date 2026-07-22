namespace Krepim.Mobile.Features.Ordering.Models.DTOs
{
    public class OrderItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
