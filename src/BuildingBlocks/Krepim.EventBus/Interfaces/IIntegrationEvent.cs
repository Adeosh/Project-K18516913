namespace Krepim.EventBus.Interfaces
{
    public interface IIntegrationEvent
    {
        Guid EventId { get; }
        DateTime OccurredOn { get; }
    }
}
