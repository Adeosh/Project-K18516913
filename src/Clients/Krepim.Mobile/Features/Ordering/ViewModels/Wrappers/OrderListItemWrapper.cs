using Krepim.Mobile.Features.Ordering.Models.DTOs;

namespace Krepim.Mobile.Features.Ordering.ViewModels.Wrappers
{
    public class OrderListItemWrapper
    {
        public OrderDto Dto { get; }

        public OrderListItemWrapper(OrderDto dto)
        {
            Dto = dto;
        }

        public string ShortId
        {
            get
            {
                if (string.IsNullOrEmpty(Dto.Id)) return string.Empty;
                var dashIndex = Dto.Id.IndexOf('-');
                return dashIndex > 0 ? Dto.Id.Substring(0, dashIndex) : Dto.Id;
            }
        }

        public string DisplayStatus => Dto.Status switch
        {
            "Pending" => "Ожидает оплаты",
            "AwaitingValidation" => "Проверка остатков",
            "Paid" => "Оплачен",
            "Shipped" => "Отправлен",
            "Cancelled" => "Отменен",
            _ => Dto.Status ?? "Неизвестно"
        };
    }
}
