using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Krepim.Mobile.Features.Catalog.Models.DTOs;
using Krepim.Mobile.Features.Catalog.Services;
using System.Collections.ObjectModel;

namespace Krepim.Mobile.Features.Catalog.ViewModels
{
    [QueryProperty(nameof(SearchQuery), "search")]
    public partial class CatalogViewModel : ObservableObject
    {
        private readonly CatalogService _catalogService;
        private int _currentPage = 1;
        private bool _hasNextPage = true;

        [ObservableProperty]
        public partial ObservableCollection<ProductDto> Products { get; set; } = new();

        [ObservableProperty]
        public partial ObservableCollection<CategoryDto> Categories { get; set; } = new();

        [ObservableProperty]
        public partial CategoryDto? SelectedCategory { get; set; }

        [ObservableProperty]
        public partial string SearchQuery { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [ObservableProperty]
        public partial int TotalCount { get; set; }

        public CatalogViewModel(CatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        partial void OnSearchQueryChanged(string value)
        {
            SearchCommand.Execute(null);
        }

        partial void OnSelectedCategoryChanged(CategoryDto? value)
        {
            SearchCommand.Execute(null);
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

            SelectedCategory = Categories.First();

            await LoadProductsAsync(1);
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
            if (Application.Current?.Windows.Count > 0 && Application.Current.Windows[0].Page != null)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert("В корзину", $"Добавлен {product.Name}", "ОК");
            }
        }
    }
}
