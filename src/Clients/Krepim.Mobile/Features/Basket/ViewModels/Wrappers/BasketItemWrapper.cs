using CommunityToolkit.Mvvm.ComponentModel;
using Krepim.Mobile.Features.Basket.Models.DTOs;

namespace Krepim.Mobile.Features.Basket.ViewModels.Wrappers
{
    public partial class BasketItemWrapper : ObservableObject
    {
        public BasketItemDto Dto { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPrice))]
        public partial int Quantity { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPrice))]
        public partial decimal Price { get; set; }

        public decimal TotalPrice => Price * Quantity;

        public BasketItemWrapper(BasketItemDto dto)
        {
            Dto = dto;
            Quantity = dto.Quantity;
            Price = dto.Price;
        }
    }
}
