using Krepim.Catalog.Application.Models;
using Krepim.EventBus.Events.Catalog;
using MassTransit;
using MongoDB.Driver;

namespace Krepim.Catalog.Infrastructure.Consumers
{
    internal sealed class ProductUpdatedConsumer(IMongoDatabase mongoDatabase) : IConsumer<ProductUpdatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<ProductUpdatedIntegrationEvent> context)
        {
            var msg = context.Message;
            var collection = mongoDatabase.GetCollection<ProductReadModel>("ProductsView");

            var update = Builders<ProductReadModel>.Update
                .Set(p => p.Name, msg.Name)
                .Set(p => p.Description, msg.Description)
                .Set(p => p.CategoryId, msg.CategoryId);

            await collection.UpdateOneAsync(x => x.Id == msg.ProductId, update);
        }
    }
}
