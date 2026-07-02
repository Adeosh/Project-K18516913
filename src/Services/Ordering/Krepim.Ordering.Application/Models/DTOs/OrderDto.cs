namespace Krepim.Ordering.Application.Models.DTOs
{
    public sealed record OrderDto(
        Guid Id,
        string Status,
        decimal TotalPrice,
        string FullAddress,
        double? Latitude,
        double? Longitude,
        string? Flat,
        DateTime CreatedAt,
        List<OrderItemDto> Items,
        string? QrCodeUrl = null);
}
