using Krepim.Catalog.Application.Models.DTOs;
using Krepim.EventBus.Events.Catalog;
using MassTransit;
using MongoDB.Driver;

namespace Krepim.Catalog.Infrastructure.Consumers
{
    internal sealed class ProductCreatedConsumer(IMongoDatabase mongoDatabase) : IConsumer<ProductCreatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
        {
            ProductCreatedIntegrationEvent message = context.Message;
            IMongoCollection<ProductReadDto> collection = mongoDatabase.GetCollection<ProductReadDto>("ProductsView");

            ProductReadDto readModel = new ProductReadDto(
                Id: message.ProductId,
                Name: message.Name,
                Description: message.Description,
                Sku: message.Sku,
                Price: message.PriceAmount,
                Currency: message.PriceCurrency,
                CategoryId: message.CategoryId,
                IsActive: false,
                ImageUrls: message.ImageUrls ?? Array.Empty<string>(),
                Standard: message.Standard,
                SalesUnit: message.SalesUnit,
                SalesStep: message.SalesStep,
                Attributes: message.Attributes ?? new Dictionary<string, string>(),
                PriceTiers: message.PriceTiers?.Select(pt => new PriceTierDto(pt.MinQuantity, pt.Amount, pt.Currency)).ToList() ?? new List<PriceTierDto>()
            );

            FilterDefinition<ProductReadDto> filter = Builders<ProductReadDto>.Filter.Eq(x => x.Id, readModel.Id);

            await collection.ReplaceOneAsync(filter, readModel, new ReplaceOptions { IsUpsert = true });
        }
    }
}
