namespace Krepim.EventBus.Events.Basket
{
    public sealed record BasketCheckoutIntegrationEvent(
        Guid OrderId,
        Guid UserId,
        string CustomerEmail,
        string? CustomerPhone,
        decimal TotalPrice,
        string FullAddress,
        double? Latitude,
        double? Longitude,
        string? Flat,
        List<BasketCheckoutItem> Items
    );
}
