namespace Krepim.Ordering.Application.Models
{
    public sealed record OrderModel(
        Guid Id,
        string Status,
        decimal TotalPrice,
        string City,
        string Street,
        string ZipCode,
        DateTime CreatedAt,
        List<OrderItemModel> Items);
}
