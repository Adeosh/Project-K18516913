namespace Krepim.Ordering.Application.Models
{
    public sealed record OrderItemModel(Guid ProductId, decimal UnitPrice, int Quantity);
}
