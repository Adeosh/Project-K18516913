using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Krepim.Mobile.Exceptions;
using Krepim.Mobile.Extensions;
using Krepim.Mobile.Features.Basket.Models.Exchange;
using Krepim.Mobile.Features.Basket.Services;
using Krepim.Mobile.Features.Catalog.Models.DTOs;
using Krepim.Mobile.Features.Catalog.Models.Enums;
using Krepim.Mobile.Features.Catalog.Services;

namespace Krepim.Mobile.Features.Catalog.ViewModels
{
    [QueryProperty(nameof(ProductId), "id")]
    public partial class ProductDetailViewModel : ObservableObject
    {
        private readonly CatalogService _catalogService;
        private readonly InventoryService _inventoryService;
        private readonly BasketService _basketService;

        [ObservableProperty]
        public partial string ProductId { get; set; } = string.Empty;

        [ObservableProperty]
        public partial ProductDto? Product { get; set; }

        [ObservableProperty]
        public partial string MainImage { get; set; } = string.Empty;

        [ObservableProperty]
        public partial int AvailableStock { get; set; }

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentPrice))]
        [NotifyPropertyChangedFor(nameof(CanAddToCart))]
        [NotifyPropertyChangedFor(nameof(ActiveTier))]
        public partial int Quantity { get; set; } = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentStep))]
        public partial bool IsPackageMode { get; set; }

        public ProductDetailViewModel(CatalogService catalogService, InventoryService inventoryService, BasketService basketService)
        {
            _catalogService = catalogService;
            _inventoryService = inventoryService;
            _basketService = basketService;
        }

        partial void OnProductIdChanged(string value)
        {
            LoadDataAsync().SafeFireAndForget();
        }

        public int CurrentStep => IsPackageMode && Product?.SalesStep > 0 ? Product.SalesStep : 1;

        public bool CanAddToCart => Quantity > 0 && Quantity <= AvailableStock;

        public PriceTierDto? ActiveTier
        {
            get
            {
                if (Product?.PriceTiers == null || !Product.PriceTiers.Any()) return null;

                return Product.PriceTiers
                    .OrderByDescending(t => t.MinQuantity)
                    .FirstOrDefault(t => Quantity >= t.MinQuantity);
            }
        }

        public decimal CurrentPrice
        {
            get
            {
                if (Product == null) return 0;
                var basePrice = Product.Price;
                return ActiveTier != null && ActiveTier.Amount > 0 ? ActiveTier.Amount : basePrice;
            }
        }

        public string UnitText => Product?.SalesUnit switch
        {
            SalesUnit.Pack => "упак.",
            SalesUnit.Kg => "кг",
            _ => "шт."
        };

        private async Task LoadDataAsync()
        {
            if (string.IsNullOrEmpty(ProductId) || !Guid.TryParse(ProductId, out var guid)) return;

            IsLoading = true;
            try
            {
                var productTask = _catalogService.GetByIdAsync(guid);
                var stockTask = _inventoryService.GetStockAsync(guid);

                await Task.WhenAll(productTask, stockTask);

                Product = await productTask;
                AvailableStock = await stockTask;

                MainImage = Product?.ImageUrls.FirstOrDefault() ?? "placeholder.png";
                Quantity = 0;
                IsPackageMode = false;

                OnPropertyChanged(nameof(CurrentPrice));
                OnPropertyChanged(nameof(ActiveTier));
                OnPropertyChanged(nameof(UnitText));
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void SetMainImage(string url)
        {
            MainImage = url;
        }

        [RelayCommand]
        public void TogglePackageMode(bool isPackage)
        {
            IsPackageMode = isPackage;

            if (IsPackageMode && Quantity > 0 && Product != null)
            {
                var remainder = Quantity % Product.SalesStep;
                if (remainder != 0)
                    Quantity += (Product.SalesStep - remainder);
            }
        }

        [RelayCommand]
        public void ChangeQuantity(int deltaMultiplier)
        {
            int delta = deltaMultiplier * CurrentStep;
            int next = Math.Max(0, Quantity + delta);

            if (next > AvailableStock) next = AvailableStock;

            Quantity = next;
        }

        [RelayCommand]
        public void IncreaseQuantity()
        {
            int delta = CurrentStep;
            int next = Math.Max(0, Quantity + delta);
            if (next > AvailableStock) next = AvailableStock;
            Quantity = next;
        }

        [RelayCommand]
        public void DecreaseQuantity()
        {
            int delta = CurrentStep;
            int next = Math.Max(0, Quantity - delta);
            if (next > AvailableStock) next = AvailableStock;
            Quantity = next;
        }

        [RelayCommand]
        public void SetUnitModeSingle()
        {
            TogglePackageMode(false);
        }

        [RelayCommand]
        public void SetUnitModePackage()
        {
            TogglePackageMode(true);
        }

        [RelayCommand]
        public async Task AddToBasketAsync()
        {
            if (!CanAddToCart || Product == null) return;

            try
            {
                var request = new AddBasketItemRequest(
                    Product.Id.ToString(),
                    Product.Name,
                    Product.Sku,
                    CurrentPrice,
                    Quantity
                );

                await _basketService.AddItemAsync(request);

                await Shell.Current.DisplayAlertAsync("Корзина", $"Добавлено: {Quantity} {UnitText}\nНа сумму: {Quantity * CurrentPrice} ₽", "ОК");

                Quantity = 0;
            }
            catch (ApiException apiEx)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", apiEx.ToUserFriendlyMessage(), "ОК");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось добавить товар: " + ex.Message, "ОК");
            }
        }
    }
}
