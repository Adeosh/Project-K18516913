using Krepim.Catalog.Application.Models;
using Krepim.EventBus.Events.Catalog;
using MassTransit;
using MongoDB.Driver;

namespace Krepim.Catalog.Infrastructure.Consumers
{
    internal sealed class ProductStatusChangedConsumer(IMongoDatabase mongoDatabase) : IConsumer<ProductStatusChangedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<ProductStatusChangedIntegrationEvent> context)
        {
            var collection = mongoDatabase.GetCollection<ProductReadModel>("ProductsView");
            var update = Builders<ProductReadModel>.Update.Set(p => p.IsActive, context.Message.IsActive);

            await collection.UpdateOneAsync(x => x.Id == context.Message.ProductId, update);
        }
    }
}
