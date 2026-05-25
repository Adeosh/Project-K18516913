namespace Krepim.Inventory.Application.Models
{
    public sealed record StockModel(Guid ProductId, int AvailableQuantity);
}
