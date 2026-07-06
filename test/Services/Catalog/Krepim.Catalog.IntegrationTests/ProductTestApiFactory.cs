using FluentAssertions;
using Krepim.Catalog.Application.Features.CreateProduct;
using Krepim.Catalog.Application.Models.DTOs;
using Krepim.Catalog.Domain.Enums;
using System.Net;
using System.Net.Http.Json;

namespace Krepim.Catalog.IntegrationTests.Helpers
{
    public class ProductTestHelper
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "/api/products";
        private readonly List<Guid> _createdProductIds = new();

        public ProductTestHelper(HttpClient client)
        {
            _client = client;
        }

        public async Task<Guid> CreateTestProduct(
            string name,
            string sku,
            bool isActive = true,
            decimal price = 10.0m,
            string[]? imageUrls = null)
        {
            // Убеждаемся, что мы авторизованы как Manager для создания продукта
            _client.DefaultRequestHeaders.Remove("X-Test-Role");
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Manager");

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

            // Если продукт должен быть неактивным, деактивируем его
            if (!isActive)
            {
                await DeactivateProduct(productId);
            }

            return productId;
        }

        public async Task DeactivateProduct(Guid productId)
        {
            _client.DefaultRequestHeaders.Remove("X-Test-Role");
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Manager");

            var deactivateResponse = await _client.PatchAsync($"{BaseUrl}/{productId}/deactivate", null);
            deactivateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        public async Task DeleteProduct(Guid productId)
        {
            _client.DefaultRequestHeaders.Remove("X-Test-Role");
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Manager");

            var deleteResponse = await _client.DeleteAsync($"{BaseUrl}/{productId}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        public async Task CreateMultipleProducts(params (string name, string sku, bool isActive)[] products)
        {
            foreach (var (name, sku, isActive) in products)
            {
                await CreateTestProduct(name, sku, isActive);
            }
        }

        public async Task ClearHeaders()
        {
            _client.DefaultRequestHeaders.Clear();
        }

        public async Task<HttpResponseMessage> SearchProducts(string searchTerm, int page = 1)
        {
            var encodedTerm = Uri.EscapeDataString(searchTerm);
            return await _client.GetAsync($"/public/search?term={encodedTerm}&page={page}");
        }

        public async Task<HttpResponseMessage> GetProductById(Guid productId)
        {
            return await _client.GetAsync($"/public/{productId}");
        }

        public async Task<List<ProductReadDto>> SearchProductsAndGetResult(string searchTerm, int page = 1)
        {
            var response = await SearchProducts(searchTerm, page);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            return await response.Content.ReadFromJsonAsync<List<ProductReadDto>>() ?? new List<ProductReadDto>();
        }

        public async Task<ProductReadDto?> GetProductByIdAndGetResult(Guid productId)
        {
            var response = await GetProductById(productId);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            return await response.Content.ReadFromJsonAsync<ProductReadDto>();
        }

        public async Task CleanupCreatedProducts()
        {
            foreach (var productId in _createdProductIds)
            {
                try
                {
                    _client.DefaultRequestHeaders.Remove("X-Test-Role");
                    _client.DefaultRequestHeaders.Add("X-Test-Role", "Manager");
                    await _client.DeleteAsync($"{BaseUrl}/{productId}");
                }
                catch
                {
                    // Игнорируем ошибки при очистке
                }
            }
            _createdProductIds.Clear();
        }

        public static CreateProductCommand CreateDefaultCommand(string name, string sku)
        {
            return new CreateProductCommand(
                Name: name,
                Description: $"Описание для {name}",
                Sku: sku,
                Price: 10.0m,
                CategoryId: Guid.Parse("4d6804ce-708d-4fd3-994f-641d65ebbae5"),
                ImageUrls: new[] { "http://localhost:9000/test-image.jpg" },
                Standard: "DIN 933",
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 1m,
                Attributes: new Dictionary<string, string> { { "Тестовый атрибут", "Значение" } },
                PriceTiers: new List<PriceTierDto> { new(100, 8.0m, "RUB") }
            );
        }
    }
}