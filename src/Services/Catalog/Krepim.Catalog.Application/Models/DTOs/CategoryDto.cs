using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Krepim.Catalog.Application.Models.DTOs
{
    public record CategoryDto(
        [property: BsonGuidRepresentation(GuidRepresentation.Standard)]
        Guid Id,
        string Name,
        string Description);
}
