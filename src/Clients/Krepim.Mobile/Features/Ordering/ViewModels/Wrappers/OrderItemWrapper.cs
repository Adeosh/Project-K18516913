using CommunityToolkit.Mvvm.ComponentModel;
using Krepim.Mobile.Features.Ordering.Models.DTOs;

namespace Krepim.Mobile.Features.Ordering.ViewModels.Wrappers
{
    public partial class OrderItemWrapper : ObservableObject
    {
        public OrderItemDto Dto { get; }

        [ObservableProperty]
        public partial string ProductName { get; set; } = "Загрузка товара...";

        [ObservableProperty]
        public partial string? ProductImageUrl { get; set; }

        public decimal TotalPrice => Dto.UnitPrice * Dto.Quantity;

        public OrderItemWrapper(OrderItemDto dto)
        {
            Dto = dto;
        }
    }
}
