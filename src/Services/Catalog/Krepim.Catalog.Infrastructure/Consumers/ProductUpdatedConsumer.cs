using Krepim.Catalog.Application.Models.DTOs;
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
            var collection = mongoDatabase.GetCollection<ProductReadDto>("ProductsView");

            var update = Builders<ProductReadDto>.Update
                .Set(p => p.Name, msg.Name)
                .Set(p => p.Description, msg.Description)
                .Set(p => p.CategoryId, msg.CategoryId)
                .Set(p => p.ImageUrls, msg.ImageUrls ?? Array.Empty<string>())
                .Set(p => p.Standard, msg.Standard)
                .Set(p => p.SalesUnit, msg.SalesUnit)
                .Set(p => p.SalesStep, msg.SalesStep)
                .Set(p => p.Attributes, msg.Attributes ?? new Dictionary<string, string>())
                .Set(p => p.PriceTiers, msg.PriceTiers?.Select(pt => new PriceTierDto(pt.MinQuantity, pt.Amount, pt.Currency)).ToList() ?? new List<PriceTierDto>());

            await collection.UpdateOneAsync(x => x.Id == msg.ProductId, update);
        }
    }
}
