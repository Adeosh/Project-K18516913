using FluentAssertions;
using Krepim.Catalog.Application.Features.CreateProduct;
using Krepim.Catalog.Application.Models.DTOs;
using Krepim.Catalog.Domain.Enums;
using Krepim.EventBus.Events.Catalog;
using MassTransit.Testing;
using System.Net;
using System.Net.Http.Json;

namespace Krepim.Catalog.IntegrationTests.Infrastructure
{
    public class ProductTestHelper
    {
        private readonly HttpClient _client;
        private readonly ITestHarness _harness;
        private const string BaseUrl = "/api/products";
        private readonly List<Guid> _createdProductIds = new();

        public ProductTestHelper(HttpClient client, ITestHarness harness)
        {
            _client = client;
            _harness = harness;
        }

        public async Task<Guid> CreateTestProductAsync(
            string name,
            string sku,
            bool isActive = true,
            decimal price = 10.0m,
            string[]? imageUrls = null)
        {
            SetManagerRole();

            var command = new CreateProductCommand(
                Name: name,
                Description: $"Описание для {name}",
                Sku: sku,
                Price: price,
                CategoryId: Guid.Parse("4d6804ce-708d-4fd3-994f-641d65ebbae5"),
                ImageUrls: imageUrls ?? new[] { "http://localhost:9000/test-image.jpg" },
                Standard: "DIN 933",
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 1m,
                Attributes: new Dictionary<string, string> { { "Тестовый атрибут", "Значение" } },
                PriceTiers: new List<PriceTierDto> { new(100, 8.0m, "RUB") }
            );

            var response = await _client.PostAsJsonAsync(BaseUrl, command);

            if (response.StatusCode != HttpStatusCode.Created)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create product: {response.StatusCode}. Error: {error}");
            }

            var productId = await response.Content.ReadFromJsonAsync<Guid>();
            productId.Should().NotBeEmpty();
            _createdProductIds.Add(productId);

            if (isActive)
                await PublishProductAsync(productId);

            return productId;
        }

        public async Task PublishProductAsync(Guid productId)
        {
            SetManagerRole();
            var response = await _client.PostAsync($"{BaseUrl}/manager/{productId}/publish", null);
            await _harness.Consumed.Any<ProductStatusChangedIntegrationEvent>(e =>
                e.Context.Message.ProductId == productId &&
                e.Context.Message.IsActive == true);
        }

        public async Task DeactivateProductAsync(Guid productId)
        {
            SetManagerRole();
            var response = await _client.PostAsync($"{BaseUrl}/manager/{productId}/deactivate", null);

            await _harness.Consumed.Any<ProductStatusChangedIntegrationEvent>(e =>
                e.Context.Message.ProductId == productId &&
                e.Context.Message.IsActive == false);
        }

        public async Task DeleteProductAsync(Guid productId)
        {
            SetManagerRole();
            var response = await _client.DeleteAsync($"{BaseUrl}/manager/{productId}");

            await _harness.Consumed.Any<ProductDeletedIntegrationEvent>(e =>
                e.Context.Message.ProductId == productId);
        }

        public async Task CreateMultipleProductsAsync(params (string name, string sku, bool isActive)[] products)
        {
            foreach (var (name, sku, isActive) in products)
                await CreateTestProductAsync(name, sku, isActive);
        }

        public async Task ClearHeadersAsync()
        {
            _client.DefaultRequestHeaders.Clear();
        }

        private void SetManagerRole()
        {
            _client.DefaultRequestHeaders.Remove("X-Test-Role");
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Manager");
        }

        public async Task<HttpResponseMessage> SearchProductsAsync(string searchTerm, int page = 1)
        {
            var encodedTerm = Uri.EscapeDataString(searchTerm);
            return await _client.GetAsync($"{BaseUrl}/public/search?term={encodedTerm}&page={page}");
        }

        public async Task<HttpResponseMessage> GetProductByIdAsync(Guid productId)
        {
            return await _client.GetAsync($"{BaseUrl}/public/{productId}");
        }

        public async Task<List<ProductReadDto>> SearchProductsAndGetResultAsync(string searchTerm, int page = 1)
        {
            var response = await SearchProductsAsync(searchTerm, page);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            return await response.Content.ReadFromJsonAsync<List<ProductReadDto>>() ?? new List<ProductReadDto>();
        }

        public async Task<ProductReadDto?> GetProductByIdAndGetResultAsync(Guid productId)
        {
            ProductReadDto? product = null;

            await FluentActions.Awaiting(async () =>
            {
                var response = await GetProductByIdAsync(productId);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new Exception("Product not found yet");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                product = await response.Content.ReadFromJsonAsync<ProductReadDto>();
                product.Should().NotBeNull();
            })
            .Should().NotThrowAsync(because: "товар должен появиться в read-модели после обработки событий");

            return product;
        }

        public async Task CleanupCreatedProductsAsync()
        {
            foreach (var productId in _createdProductIds.ToList())
            {
                try
                {
                    SetManagerRole();
                    await _client.DeleteAsync($"{BaseUrl}/manager/{productId}");
                }
                catch { }
            }
            _createdProductIds.Clear();
        }
    }

    public class UploadResponse { public List<string> Urls { get; set; } = new(); }
}
