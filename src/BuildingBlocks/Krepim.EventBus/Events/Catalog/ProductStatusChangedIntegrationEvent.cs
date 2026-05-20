using Krepim.EventBus.Interfaces;

namespace Krepim.EventBus.Events.Catalog
{
    public sealed record ProductStatusChangedIntegrationEvent(
        Guid ProductId, 
        bool IsActive) : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }
}
