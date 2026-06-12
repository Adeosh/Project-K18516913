namespace Krepim.Inventory.Application.Models.DTOs
{
    public sealed record StockDto(Guid ProductId, int AvailableQuantity);
}
