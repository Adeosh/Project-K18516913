using Krepim.EventBus.Interfaces;

namespace Krepim.EventBus.Events.Catalog
{
    public sealed record ProductDeletedIntegrationEvent(Guid ProductId) : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }
}
