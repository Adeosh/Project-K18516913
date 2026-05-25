namespace Krepim.EventBus.Events.Inventory
{
    public sealed record OrderCreatedIntegrationEvent(
        Guid OrderId,
        List<OrderItemPayload> Items
    );
}
