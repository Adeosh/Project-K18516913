using Krepim.Catalog.Application.Models.DTOs;
using Krepim.EventBus.Events.Catalog;
using MassTransit;
using MongoDB.Driver;

namespace Krepim.Catalog.Infrastructure.Consumers
{
    internal sealed class ProductStatusChangedConsumer(IMongoDatabase mongoDatabase) : IConsumer<ProductStatusChangedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<ProductStatusChangedIntegrationEvent> context)
        {
            var collection = mongoDatabase.GetCollection<ProductReadDto>("ProductsView");
            var update = Builders<ProductReadDto>.Update.Set(p => p.IsActive, context.Message.IsActive);

            await collection.UpdateOneAsync(x => x.Id == context.Message.ProductId, update);
        }
    }
}
