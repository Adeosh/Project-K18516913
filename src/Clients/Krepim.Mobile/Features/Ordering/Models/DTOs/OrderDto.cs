namespace Krepim.Mobile.Features.Ordering.Models.DTOs
{
    public class OrderDto
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string FullAddress { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Flat { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? QrCodeUrl { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
