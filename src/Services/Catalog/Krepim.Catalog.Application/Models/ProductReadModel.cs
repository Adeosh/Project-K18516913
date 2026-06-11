using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Krepim.Catalog.Application.Models
{
    public record ProductReadModel(
        [property: BsonId]
        [property: BsonGuidRepresentation(GuidRepresentation.Standard)]
        Guid Id,
        string Name,
        string Description,
        string Sku,
        decimal Price,
        string Currency,
        [property: BsonGuidRepresentation(GuidRepresentation.Standard)]
        Guid CategoryId,
        bool IsActive,
        IReadOnlyList<string> ImageUrls
    );
}
