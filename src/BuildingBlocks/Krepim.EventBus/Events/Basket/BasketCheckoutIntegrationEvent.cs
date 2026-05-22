namespace Krepim.EventBus.Events.Basket
{
    public sealed record BasketCheckoutIntegrationEvent(
        Guid UserId,
        decimal TotalPrice,
        string City,
        string Street,
        string ZipCode,
        List<BasketCheckoutItem> Items
    );
}
