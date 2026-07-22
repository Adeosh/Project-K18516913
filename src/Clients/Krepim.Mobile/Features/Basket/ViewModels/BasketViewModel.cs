using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Krepim.Mobile.Exceptions;
using Krepim.Mobile.Extensions;
using Krepim.Mobile.Features.Auth.Services;
using Krepim.Mobile.Features.Basket.Models.Exchange;
using Krepim.Mobile.Features.Basket.Services;
using Krepim.Mobile.Features.Basket.ViewModels.Wrappers;
using Krepim.Mobile.Features.Catalog.Services;
using System.Collections.ObjectModel;

namespace Krepim.Mobile.Features.Basket.ViewModels
{
    public partial class BasketViewModel : ObservableObject
    {
        private readonly BasketService _basketService;
        private readonly AuthService _authService;
        private readonly CatalogService _catalogService;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEmpty))]
        [NotifyPropertyChangedFor(nameof(TotalSum))]
        public partial ObservableCollection<BasketItemWrapper> BasketItems { get; set; } = new();

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [ObservableProperty]
        public partial bool IsCheckingOut { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;

        public bool IsEmpty => BasketItems.Count == 0;
        public decimal TotalSum => BasketItems.Sum(i => i.TotalPrice);

        public BasketViewModel(BasketService basketService, AuthService authService, CatalogService catalogService)
        {
            _basketService = basketService;
            _authService = authService;
            _catalogService = catalogService;
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var basket = await _basketService.GetBasketAsync();

                BasketItems.Clear();
                foreach (var item in basket.Items)
                    BasketItems.Add(new BasketItemWrapper(item));

                RefreshUi();
            }
            catch (ApiException apiEx)
            {
                ErrorMessage = apiEx.ToUserFriendlyMessage();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка загрузки корзины: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task IncreaseQuantityAsync(BasketItemWrapper wrapper)
        {
            if (wrapper == null) return;
            wrapper.Quantity++;
            await UpdateItemAsync(wrapper);
        }

        [RelayCommand]
        public async Task DecreaseQuantityAsync(BasketItemWrapper wrapper)
        {
            if (wrapper == null || wrapper.Quantity <= 1) return;
            wrapper.Quantity--;
            await UpdateItemAsync(wrapper);
        }

        [RelayCommand]
        public async Task RemoveItemAsync(string productId)
        {
            ErrorMessage = string.Empty;
            var wrapper = BasketItems.FirstOrDefault(i => i.Dto.ProductId == productId);

            if (wrapper != null)
            {
                BasketItems.Remove(wrapper);
                RefreshUi();

                try
                {
                    await _basketService.RemoveItemAsync(productId);
                }
                catch (ApiException apiEx)
                {
                    ErrorMessage = apiEx.ToUserFriendlyMessage();
                    BasketItems.Add(wrapper);
                    RefreshUi();
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Ошибка удаления: " + ex.Message;
                    BasketItems.Add(wrapper);
                    RefreshUi();
                }
            }
        }

        [RelayCommand]
        public async Task CheckoutAsync()
        {
            ErrorMessage = string.Empty;

            try
            {
                var profile = await _authService.GetProfileAsync();

                if (string.IsNullOrWhiteSpace(profile?.DefaultAddress?.FullAddress))
                {
                    ErrorMessage = "Пожалуйста, заполните адрес доставки в профиле.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(profile.Email))
                {
                    ErrorMessage = "Пожалуйста, укажите email в профиле.";
                    return;
                }

                IsCheckingOut = true;

                var request = new CheckoutRequest(
                    profile.Email,
                    profile.PhoneNumber,
                    profile.DefaultAddress.FullAddress,
                    profile.DefaultAddress.Latitude,
                    profile.DefaultAddress.Longitude,
                    profile.DefaultAddress.Flat
                );

                var orderId = await _basketService.CheckoutAsync(request);

                BasketItems.Clear();
                RefreshUi();
                await Shell.Current.GoToAsync($"///OrderSuccessPage?orderId={orderId}");
            }
            catch (ApiException apiEx)
            {
                ErrorMessage = apiEx.ToUserFriendlyMessage();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка оформления заказа: " + ex.Message;
            }
            finally
            {
                IsCheckingOut = false;
            }
        }

        [RelayCommand]
        public async Task GoToCatalogAsync()
        {
            await Shell.Current.GoToAsync("///CatalogPage");
        }

        private async Task UpdateItemAsync(BasketItemWrapper wrapper)
        {
            wrapper.Dto.Quantity = wrapper.Quantity;
            ErrorMessage = string.Empty;

            try
            {
                var product = await _catalogService.GetByIdAsync(Guid.Parse(wrapper.Dto.ProductId));
                if (product != null)
                    wrapper.Price = product.Price;

                RefreshUi();
                await _basketService.UpdateQuantityAsync(wrapper.Dto.ProductId, wrapper.Quantity, wrapper.Price);
            }
            catch (ApiException apiEx)
            {
                ErrorMessage = apiEx.ToUserFriendlyMessage();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка обновления количества: " + ex.Message;
            }
        }

        private void RefreshUi()
        {
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(TotalSum));
        }
    }
}
