using Krepim.Catalog.Application.Models;
using Krepim.EventBus.Events.Catalog;
using MassTransit;
using MongoDB.Driver;

namespace Krepim.Catalog.Infrastructure.Consumers
{
    internal sealed class ProductCreatedConsumer(IMongoDatabase mongoDatabase) : IConsumer<ProductCreatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
        {
            var message = context.Message;
            var collection = mongoDatabase.GetCollection<ProductReadModel>("ProductsView");

            var readModel = new ProductReadModel(
                Id: message.ProductId,
                Name: message.Name,
                Description: message.Description,
                Sku: message.Sku,
                Price: message.PriceAmount,
                Currency: message.PriceCurrency,
                PrimaryImageUrl: null,
                CategoryId: message.CategoryId,
                IsActive: false
            );

            var filter = Builders<ProductReadModel>.Filter.Eq(x => x.Id, readModel.Id);

            await collection.ReplaceOneAsync(filter, readModel, new ReplaceOptions { IsUpsert = true });
        }
    }
}
