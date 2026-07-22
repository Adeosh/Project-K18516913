using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Krepim.Mobile.Exceptions;
using Krepim.Mobile.Extensions;
using Krepim.Mobile.Features.Ordering.Models.DTOs;
using Krepim.Mobile.Features.Ordering.Services;
using System.Collections.ObjectModel;

namespace Krepim.Mobile.Features.Ordering.ViewModels
{
    public partial class MyOrdersViewModel : ObservableObject
    {
        private readonly OrderService _orderService;

        [ObservableProperty]
        public partial ObservableCollection<OrderDto> Orders { get; set; } = new();

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        public bool IsEmpty => !IsLoading && Orders.Count == 0;

        public MyOrdersViewModel(OrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            try
            {
                var orders = await _orderService.GetMyOrdersAsync();
                Orders.Clear();
                foreach (var order in orders.OrderByDescending(o => o.CreatedAt))
                {
                    Orders.Add(order);
                }
                OnPropertyChanged(nameof(IsEmpty));
            }
            catch (ApiException apiEx)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", apiEx.ToUserFriendlyMessage(), "ОК");
            }
            finally
            {
                IsLoading = false;
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
