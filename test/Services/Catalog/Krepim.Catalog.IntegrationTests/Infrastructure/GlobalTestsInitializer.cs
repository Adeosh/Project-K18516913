using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Krepim.Catalog.IntegrationTests.Infrastructure
{
    public sealed class GlobalTestsInitializer
    {
        public GlobalTestsInitializer()
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }
    }
}
