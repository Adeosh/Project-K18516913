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
            ProductUpdatedIntegrationEvent message = context.Message;
            IMongoCollection<ProductReadDto> collection = mongoDatabase.GetCollection<ProductReadDto>("ProductsView");

            var update = Builders<ProductReadDto>.Update
                .Set(p => p.Name, message.Name)
                .Set(p => p.Description, message.Description)
                .Set(p => p.CategoryId, message.CategoryId)
                .Set(p => p.ImageUrls, message.ImageUrls ?? Array.Empty<string>())
                .Set(p => p.Standard, message.Standard)
                .Set(p => p.SalesUnit, message.SalesUnit)
                .Set(p => p.SalesStep, message.SalesStep)
                .Set(p => p.Attributes, message.Attributes ?? new Dictionary<string, string>())
                .Set(p => p.PriceTiers, message.PriceTiers?.Select(pt => new PriceTierDto(pt.MinQuantity, pt.Amount, pt.Currency)).ToList() ?? new List<PriceTierDto>());

            await collection.UpdateOneAsync(x => x.Id == message.ProductId, update);
        }
    }
}
