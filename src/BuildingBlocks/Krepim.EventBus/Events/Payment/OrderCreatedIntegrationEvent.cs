using Krepim.EventBus.Events.Inventory;

namespace Krepim.EventBus.Events.Payment
{
    public sealed record OrderCreatedIntegrationEvent(
        Guid OrderId,
        decimal TotalPrice,
        List<OrderItemPayload> Items
    );
}
