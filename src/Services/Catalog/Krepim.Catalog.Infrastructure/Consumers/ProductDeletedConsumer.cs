using Krepim.Catalog.Application.Models.DTOs;
using Krepim.EventBus.Events.Catalog;
using MassTransit;
using MongoDB.Driver;

namespace Krepim.Catalog.Infrastructure.Consumers
{
    internal sealed class ProductDeletedConsumer(IMongoDatabase mongoDatabase) : IConsumer<ProductDeletedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<ProductDeletedIntegrationEvent> context)
        {
            IMongoCollection<ProductReadDto> collection = mongoDatabase.GetCollection<ProductReadDto>("ProductsView");

            await collection.DeleteOneAsync(x => x.Id == context.Message.ProductId);
        }
    }
}
