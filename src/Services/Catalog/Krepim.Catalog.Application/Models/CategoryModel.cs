using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Krepim.Catalog.Application.Models
{
    public record CategoryModel(
        [property: BsonGuidRepresentation(GuidRepresentation.Standard)]
        Guid Id, 
        string Name, 
        string Description);
}
