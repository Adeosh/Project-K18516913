using CommunityToolkit.Mvvm.ComponentModel;
using Krepim.Mobile.Features.Basket.Models.DTOs;
using Krepim.Mobile.Features.Catalog.Models.DTOs;
using Krepim.Mobile.Features.Catalog.Services;

namespace Krepim.Mobile.Features.Basket.ViewModels.Wrappers
{
    public partial class BasketItemWrapper : ObservableObject
    {
        public BasketItemDto Dto { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPrice))]
        [NotifyPropertyChangedFor(nameof(Price))]
        public partial int Quantity { get; set; }

        [ObservableProperty]
        public partial string ProductName { get; set; }

        [ObservableProperty]
        public partial string? ProductImageUrl { get; set; }

        [ObservableProperty]
        public partial string Sku { get; set; }

        public List<PriceTierDto> PriceTiers { get; set; } = new();

        public decimal Price
        {
            get
            {
                if (PriceTiers == null || PriceTiers.Count == 0)
                    return Dto.Price;

                var activeTier = PriceTiers
                    .OrderByDescending(t => t.MinQuantity)
                    .FirstOrDefault(t => Quantity >= t.MinQuantity);

                return activeTier != null ? activeTier.Amount : Dto.Price;
            }
        }

        public decimal TotalPrice => Price * Quantity;

        public BasketItemWrapper(BasketItemDto dto)
        {
            Dto = dto;
            Quantity = dto.Quantity;
            ProductName = dto.Name;
            ProductImageUrl = dto.ImageUrl;
            Sku = dto.Sku;
        }

        public void RecalculatePrice()
        {
            OnPropertyChanged(nameof(Price));
            OnPropertyChanged(nameof(TotalPrice));
        }

        public async Task LoadProductDetailsAsync(CatalogService catalogService)
        {
            try
            {
                if (Guid.TryParse(Dto.ProductId, out var productId))
                {
                    var product = await catalogService.GetByIdAsync(productId);
                    if (product != null)
                    {
                        ProductName = product.Name;
                        Sku = product.Sku;
                        Dto.Price = product.Price;

                        if (product.PriceTiers != null)
                            PriceTiers = product.PriceTiers;

                        if (product.ImageUrls?.Count > 0)
                            ProductImageUrl = product.ImageUrls.First();

                        RecalculatePrice();
                    }
                }
            }
            catch { }
        }
    }
}
