namespace Krepim.Mobile.Features.Catalog.Models.DTOs
{
    public class PriceTierDto
    {
        public int MinQuantity { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "RUB";
    }
}
