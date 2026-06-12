namespace Krepim.Ordering.Application.Models.DTOs
{
    public sealed record OrderItemDto(Guid ProductId, decimal UnitPrice, int Quantity);
}
