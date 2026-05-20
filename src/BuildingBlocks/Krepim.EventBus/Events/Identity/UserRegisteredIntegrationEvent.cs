using Krepim.EventBus.Interfaces;

namespace Krepim.EventBus.Events.Identity
{
    public sealed record UserRegisteredIntegrationEvent(
        Guid UserId,
        string Email,
        string Role) : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }
}
