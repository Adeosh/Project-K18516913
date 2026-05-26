namespace Krepim.EventBus.Events.Inventory
{
    public sealed record OrderCreatedIntegrationEvent(
        Guid OrderId,
        decimal TotalPrice,
        List<OrderItemPayload> Items
    );
}
