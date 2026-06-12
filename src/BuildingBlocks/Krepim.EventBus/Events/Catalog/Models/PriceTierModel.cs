namespace Krepim.EventBus.Events.Catalog.Models
{
    public sealed record PriceTierModel(int MinQuantity, decimal Amount, string Currency);
}
