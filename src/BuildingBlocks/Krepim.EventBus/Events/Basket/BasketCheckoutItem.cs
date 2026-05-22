namespace Krepim.EventBus.Events.Basket
{
    public sealed record BasketCheckoutItem(Guid ProductId, decimal UnitPrice, int Quantity);
}
