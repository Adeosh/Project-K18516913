using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Krepim.Mobile.Exceptions;
using Krepim.Mobile.Extensions;
using Krepim.Mobile.Features.Ordering.Models.DTOs;
using Krepim.Mobile.Features.Ordering.Services;
using System.Collections.ObjectModel;

namespace Krepim.Mobile.Features.Ordering.ViewModels
{
    public partial class OrdersDashboardViewModel : ObservableObject
    {
        private readonly OrderService _orderService;
        private List<OrderDto> _allOrders = new();

        [ObservableProperty]
        public partial ObservableCollection<OrderDto> FilteredOrders { get; set; } = new();

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string SearchQuery { get; set; } = string.Empty;

        partial void OnSearchQueryChanged(string value) => ApplyFilters();

        public OrdersDashboardViewModel(OrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                _allOrders = orders.OrderByDescending(o => o.CreatedAt).ToList();
                ApplyFilters();
            }
            catch (ApiException apiEx)
            {
                ErrorMessage = apiEx.ToUserFriendlyMessage();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка загрузки заказов: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyFilters()
        {
            var query = SearchQuery?.ToLower() ?? string.Empty;

            var filtered = _allOrders.Where(o =>
                o.Id.ToLower().Contains(query) ||
                o.FullAddress.ToLower().Contains(query) ||
                o.CustomerEmail.ToLower().Contains(query) ||
                (o.CustomerPhone != null && o.CustomerPhone.ToLower().Contains(query))
            ).ToList();

            FilteredOrders.Clear();
            foreach (var order in filtered)
            {
                FilteredOrders.Add(order);
            }
        }

        [RelayCommand]
        public async Task GoToDetailsAsync(OrderDto order)
        {
            if (order == null) return;
            await Shell.Current.GoToAsync($"OrderDetailPage?orderId={order.Id}");
        }
    }
}
