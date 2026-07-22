using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Krepim.Mobile.Exceptions;
using Krepim.Mobile.Extensions;
using Krepim.Mobile.Features.Catalog.Services;
using Krepim.Mobile.Features.Ordering.Models.DTOs;
using Krepim.Mobile.Features.Ordering.Services;
using Krepim.Mobile.Features.Ordering.ViewModels.Wrappers;
using Krepim.Mobile.Features.Payment.Services;
using System.Collections.ObjectModel;

namespace Krepim.Mobile.Features.Ordering.ViewModels
{
    [QueryProperty(nameof(OrderId), "orderId")]
    public partial class OrderDetailViewModel : ObservableObject
    {
        private readonly OrderService _orderService;
        private readonly CatalogService _catalogService;
        private readonly PaymentService _paymentService;

        [ObservableProperty]
        public partial string OrderId { get; set; } = string.Empty;

        [ObservableProperty]
        public partial OrderDto? Order { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<OrderItemWrapper> OrderItems { get; set; } = new();

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [ObservableProperty]
        public partial bool IsPaymentLoading { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string PaymentError { get; set; } = string.Empty;

        public string DisplayStatus => Order?.Status switch
        {
            "Pending" => "Ожидает оплаты",
            "AwaitingValidation" => "Проверка остатков",
            "Paid" => "Оплачен",
            "Shipped" => "Отправлен",
            "Cancelled" => "Отменен",
            _ => Order?.Status ?? "Неизвестно"
        };

        public bool CanBePaid => Order?.Status == "Pending" || Order?.Status == "AwaitingValidation";

        public OrderDetailViewModel(OrderService orderService, CatalogService catalogService, PaymentService paymentService)
        {
            _orderService = orderService;
            _catalogService = catalogService;
            _paymentService = paymentService;
        }

        public async Task InitializeAsync()
        {
            if (string.IsNullOrEmpty(OrderId)) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                Order = await _orderService.GetByIdAsync(OrderId);
                OnPropertyChanged(nameof(DisplayStatus));
                OnPropertyChanged(nameof(CanBePaid));

                OrderItems.Clear();
                foreach (var item in Order.Items)
                {
                    var wrapper = new OrderItemWrapper(item);
                    OrderItems.Add(wrapper);

                    _ = LoadProductDetailsAsync(wrapper);
                }
            }
            catch (ApiException apiEx)
            {
                ErrorMessage = apiEx.ToUserFriendlyMessage();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка загрузки заказа: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadProductDetailsAsync(OrderItemWrapper wrapper)
        {
            try
            {
                var product = await _catalogService.GetByIdAsync(Guid.Parse(wrapper.Dto.ProductId));
                if (product != null)
                {
                    wrapper.ProductName = product.Name;
                    wrapper.ProductImageUrl = product.ImageUrls?.FirstOrDefault();
                }
                else
                    wrapper.ProductName = "Товар не найден";
            }
            catch
            {
                wrapper.ProductName = "Ошибка загрузки названия";
            }
        }

        [RelayCommand]
        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task PayAsync()
        {
            if (Order == null) return;

            IsPaymentLoading = true;
            PaymentError = string.Empty;

            int attempts = 0;
            int maxAttempts = 6;

            while (attempts < maxAttempts)
            {
                try
                {
                    var url = await _paymentService.GetPaymentUrlAsync(Order.Id);

                    if (!string.IsNullOrEmpty(url))
                    {
                        if (url.Contains("/mock-pay"))
                        {
                            var uri = new Uri(url.StartsWith("http") ? url : $"http://localhost{url}");
                            await Shell.Current.GoToAsync($"///MockPayPage{uri.Query}");
                        }
                        else
                            await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);

                        IsPaymentLoading = false;
                        return;
                    }
                }
                catch
                {
                    attempts++;
                    await Task.Delay(1200);
                }
            }

            PaymentError = "Платежная система пока недоступна. Пожалуйста, попробуйте нажать кнопку еще раз чуть позже.";
            IsPaymentLoading = false;
        }
    }
}
