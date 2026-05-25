namespace Krepim.EventBus.Events.Inventory
{
    public sealed record OrderItemPayload(Guid ProductId, int Quantity);
}
