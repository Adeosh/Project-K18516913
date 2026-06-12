using Krepim.EventBus.Events.Catalog.Models;
using Krepim.EventBus.Interfaces;

namespace Krepim.EventBus.Events.Catalog
{
    public sealed record ProductUpdatedIntegrationEvent(
        Guid ProductId, 
        string Name, 
        string Description,
        decimal PriceAmount, 
        string PriceCurrency, 
        Guid CategoryId,
        string[] ImageUrls,
        string? Standard,
        int SalesUnit,
        decimal SalesStep,
        Dictionary<string, string> Attributes,
        List<PriceTierModel> PriceTiers) : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }
}
