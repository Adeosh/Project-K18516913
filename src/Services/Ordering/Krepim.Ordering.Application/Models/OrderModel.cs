namespace Krepim.Ordering.Application.Models
{
    public sealed record OrderModel(
        Guid Id,
        string Status,
        decimal TotalPrice,
        string FullAddress,
        double? Latitude,
        double? Longitude,
        string? Flat,
        DateTime CreatedAt,
        List<OrderItemModel> Items);
}
