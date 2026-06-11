using Krepim.EventBus.Interfaces;

namespace Krepim.EventBus.Events.Catalog
{
    public sealed record ProductCreatedIntegrationEvent(
        Guid ProductId,
        string Name,
        string Description,
        string Sku,
        decimal PriceAmount,
        string PriceCurrency,
        Guid CategoryId,
        string[] ImageUrls) : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }
}
