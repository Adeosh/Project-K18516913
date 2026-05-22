using FluentAssertions;
using Krepim.Catalog.Application.Models;
using Krepim.Catalog.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System.Net;
using System.Net.Http.Json;

namespace Krepim.Catalog.IntegrationTests.Features.Products
{
    public class ClientEndpointsTests : IClassFixture<CatalogWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly IMongoDatabase _mongoDb;

        public ClientEndpointsTests(CatalogWebApplicationFactory factory)
        {
            _client = factory.CreateClient();

            var scope = factory.Services.CreateScope();
            _mongoDb = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        }

        private async Task SeedMongoProductAsync(ProductReadModel product)
        {
            var collection = _mongoDb.GetCollection<ProductReadModel>("ProductsView");
            await collection.InsertOneAsync(product);
        }

        [Fact]
        public async Task GetProductById_Should_ReturnProduct_WhenItExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new ProductReadModel(
                Id: productId,
                Name: "Моторное масло 5W-40",
                Description: "Синтетика",
                Sku: "OIL-5W40",
                Price: 3500m,
                Currency: "RUB",
                PrimaryImageUrl: null,
                CategoryId: Guid.NewGuid(),
                IsActive: true
            );

            await SeedMongoProductAsync(product);

            // Act
            var response = await _client.GetAsync($"/api/products/public/{productId}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ProductReadModel>(
                cancellationToken: TestContext.Current.CancellationToken);

            result.Should().NotBeNull();
            result!.Id.Should().Be(productId);
            result.Name.Should().Be("Моторное масло 5W-40");
        }

        [Fact]
        public async Task SearchProducts_Should_ReturnMatchingProducts()
        {
            // Arrange
            await SeedMongoProductAsync(new ProductReadModel(Guid.NewGuid(), "Колодки передние", "Desc", "B-1", 1500m, "RUB", null, Guid.NewGuid(), true));
            await SeedMongoProductAsync(new ProductReadModel(Guid.NewGuid(), "Колодки задние", "Desc", "B-2", 1500m, "RUB", null, Guid.NewGuid(), true));
            await SeedMongoProductAsync(new ProductReadModel(Guid.NewGuid(), "Фильтр масляный", "Desc", "F-1", 500m, "RUB", null, Guid.NewGuid(), true));

            // Act
            var response = await _client.GetAsync(
                "/api/products/public/search?term=%D0%9A%D0%BE%D0%BB%D0%BE%D0%B4%D0%BA%D0%B8&page=1",
                TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var results = await response.Content.ReadFromJsonAsync<IReadOnlyList<ProductReadModel>>(
                cancellationToken: TestContext.Current.CancellationToken);

            results.Should().NotBeNull();
            results!.Count.Should().BeGreaterThanOrEqualTo(2);
            results.Should().AllSatisfy(p => p.Name.Contains("Колодки"));
        }

        [Fact]
        public async Task GetProductById_Should_ReturnNotFound_WhenIdDoesNotExist()
        {
            // Act
            var response = await _client.GetAsync($"/api/products/public/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
