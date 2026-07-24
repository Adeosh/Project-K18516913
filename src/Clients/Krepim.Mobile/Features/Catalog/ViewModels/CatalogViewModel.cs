using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Krepim.Mobile.Exceptions;
using Krepim.Mobile.Extensions;
using Krepim.Mobile.Features.Basket.Models.Exchange;
using Krepim.Mobile.Features.Basket.Services;
using Krepim.Mobile.Features.Catalog.Models.DTOs;
using Krepim.Mobile.Features.Catalog.Services;
using System.Collections.ObjectModel;

namespace Krepim.Mobile.Features.Catalog.ViewModels
{
    [QueryProperty(nameof(SearchQuery), "search")]
    [QueryProperty(nameof(CategoryQuery), "category")]
    public partial class CatalogViewModel : ObservableObject
    {
        private readonly CatalogService _catalogService;
        private readonly BasketService _basketService;
        private int _currentPage = 1;
        private bool _hasNextPage = true;
        private bool _isInitialized = false;

        [ObservableProperty]
        public partial ObservableCollection<ProductDto> Products { get; set; } = new();

        [ObservableProperty]
        public partial ObservableCollection<CategoryDto> Categories { get; set; } = new();

        [ObservableProperty]
        public partial CategoryDto? SelectedCategory { get; set; }

        [ObservableProperty]
        public partial string CategoryQuery { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string SearchQuery { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [ObservableProperty]
        public partial int TotalCount { get; set; }

        public CatalogViewModel(CatalogService catalogService, BasketService basketService)
        {
            _catalogService = catalogService;
            _basketService = basketService;
        }

        partial void OnSearchQueryChanged(string value)
        {
            if (!_isInitialized) return;
            SearchCommand.Execute(null);
        }

        partial void OnSelectedCategoryChanged(CategoryDto? value)
        {
            if (!_isInitialized) return;
            SearchCommand.Execute(null);
        }

        partial void OnCategoryQueryChanged(string value)
        {
            if (!_isInitialized) return;

            if (string.IsNullOrWhiteSpace(value) || Categories == null || !Categories.Any())
                return;

            var matchedCat = Categories.FirstOrDefault(c => c.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (matchedCat != null)
                SelectedCategory = matchedCat;
        }

        [RelayCommand]
        public async Task InitializeAsync()
        {
            if (Categories.Any()) return;

            var cats = await _catalogService.GetCategoriesAsync();
            Categories.Clear();
            Categories.Add(new CategoryDto { Id = Guid.Empty, Name = "Все категории" });

            foreach (var c in cats)
                Categories.Add(c);

            CategoryDto targetCategory = Categories.First();

            if (!string.IsNullOrWhiteSpace(CategoryQuery))
            {
                var matchedCat = Categories.FirstOrDefault(c => c.Name.Equals(CategoryQuery, StringComparison.OrdinalIgnoreCase));
                if (matchedCat != null)
                    targetCategory = matchedCat;

                CategoryQuery = string.Empty;
            }

            _isInitialized = true;
            SelectedCategory = targetCategory;
        }

        [RelayCommand]
        public async Task SearchAsync()
        {
            await LoadProductsAsync(1);
        }

        [RelayCommand]
        public async Task LoadMoreAsync()
        {
            if (IsLoading || !_hasNextPage) return;
            await LoadProductsAsync(_currentPage + 1);
        }

        private async Task LoadProductsAsync(int page)
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                var catId = SelectedCategory?.Id == Guid.Empty ? null : (Guid?)SelectedCategory?.Id;

                var result = await _catalogService.SearchAsync(SearchQuery, catId, page, 8);

                if (page == 1)
                    Products.Clear();

                foreach (var item in result.Items)
                    Products.Add(item);

                _currentPage = result.Page;
                TotalCount = result.TotalCount;
                _hasNextPage = result.Items.Count >= result.PageSize;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task GoToProductAsync(ProductDto product)
        {
            await Shell.Current.GoToAsync($"product?id={product.Id}");
        }

        [RelayCommand]
        public async Task AddToBasketAsync(ProductDto product)
        {
            if (product == null) return;

            try
            {
                var request = new AddBasketItemRequest(
                    product.Id.ToString(),
                    product.Name,
                    product.Sku,
                    product.Price,
                    product.SalesStep
                );

                await _basketService.AddItemAsync(request);
                await Shell.Current.DisplayAlertAsync("Успешно", $"{product.Name} добавлен в корзину", "ОК");
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
