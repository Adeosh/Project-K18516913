using Krepim.Mobile.Features.Catalog.Models.Enums;

namespace Krepim.Mobile.Features.Catalog.Models.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public bool IsActive { get; set; }
        public string? Standard { get; set; }
        public SalesUnit SalesUnit { get; set; } = SalesUnit.Pcs;
        public int SalesStep { get; set; } = 1;

        public Dictionary<string, string> Attributes { get; set; } = new();
        public List<PriceTierDto> PriceTiers { get; set; } = new();
    }
}
