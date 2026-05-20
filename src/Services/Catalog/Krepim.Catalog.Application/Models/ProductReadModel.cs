namespace Krepim.Catalog.Application.Models
{
    public record ProductReadModel(
        Guid Id,
        string Name,
        string Description,
        string Sku,
        decimal Price,
        string Currency,
        string? PrimaryImageUrl,
        Guid CategoryId,
        bool IsActive
    );
}
