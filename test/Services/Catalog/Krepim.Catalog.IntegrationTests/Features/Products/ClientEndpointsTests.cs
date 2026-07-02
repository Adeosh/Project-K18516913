using FluentAssertions;
using Krepim.Catalog.Application.Models.DTOs;
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

        private async Task SeedMongoProductAsync(ProductReadDto product)
        {
            var collection = _mongoDb.GetCollection<ProductReadDto>("ProductsView");
            await collection.InsertOneAsync(product);
        }

        [Fact]
        public async Task GetProductById_Should_ReturnProduct_WhenItExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new ProductReadDto(
                Id: productId,
                Name: "Моторное масло 5W-40",
                Description: "Синтетика",
                Sku: "OIL-5W40",
                Price: 3500m,
                Currency: "RUB",
                CategoryId: Guid.NewGuid(),
                IsActive: true,
                ImageUrls: new[]
                {
                    "http://localhost:9000/catalog-images/0780d3b0-b469-44f2-9ca0-25645206f1c0-7492748982.jpeg",
                    "http://localhost:9000/catalog-images/2e3733dd-d760-483f-9970-c266b854f9c1-7492748983.jpeg"
                },
                Standard: "DIN 7338",
                SalesUnit: 1,
                SalesStep: 250m,
                Attributes: new Dictionary<string, string>
                {
                    { "Длина (L)", "15 мм" },
                    { "Материал", "Оцинкованная сталь" },
                    { "Тип буртика", "Уменьшенный (потайной)" },
                    { "Форма корпуса", "Цилиндрическая с насечкой" },
                    { "Диаметр резьбы (d)", "М6" },
                    { "Толщина скрепляемых материалов", "1.5 - 3.0 мм" }
                },
                PriceTiers: new List<PriceTierDto>
                {
                    new(1000, 3.8m, "RUB"),
                    new(5000, 3.1m, "RUB"),
                    new(20000, 2.65m, "RUB")
                });

            await SeedMongoProductAsync(product);

            // Act
            var response = await _client.GetAsync($"/api/products/public/{productId}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ProductReadDto>(
                cancellationToken: TestContext.Current.CancellationToken);

            result.Should().NotBeNull();
            result!.Id.Should().Be(productId);
            result.Name.Should().Be("Моторное масло 5W-40");
        }

        [Fact]
        public async Task SearchProducts_Should_ReturnMatchingProducts()
        {
            // Arrange
            await SeedMongoProductAsync(new ProductReadDto(
                Id: Guid.NewGuid(),
                Name: "Заклепка резьбовая цилиндрическая с малым фланцем М6х15",
                Description: "Заклепка резьбовая цилиндрическая с малым фланцем",
                Sku: "ZRM-M6X15-ST-ZN",
                Price: 4.50m,
                Currency: "RUB",
                CategoryId: Guid.NewGuid(),
                IsActive: true,
                ImageUrls: new[]
                {
                    "http://localhost:9000/catalog-images/0780d3b0-b469-44f2-9ca0-25645206f1c0-7492748982.jpeg",
                    "http://localhost:9000/catalog-images/2e3733dd-d760-483f-9970-c266b854f9c1-7492748983.jpeg"
                },
                Standard: "DIN 7338",
                SalesUnit: 1,
                SalesStep: 250m,
                Attributes: new Dictionary<string, string>
                {
                    { "Длина (L)", "15 мм" },
                    { "Материал", "Оцинкованная сталь" },
                    { "Тип буртика", "Уменьшенный (потайной)" },
                    { "Форма корпуса", "Цилиндрическая с насечкой" },
                    { "Диаметр резьбы (d)", "М6" },
                    { "Толщина скрепляемых материалов", "1.5 - 3.0 мм" }
                },
                PriceTiers: new List<PriceTierDto>
                {
                    new(1000, 3.8m, "RUB"),
                    new(5000, 3.1m, "RUB"),
                    new(20000, 2.65m, "RUB")
                }));

            await SeedMongoProductAsync(new ProductReadDto(
                Id: Guid.NewGuid(),
                Name: "Колодки тормозные задние дисковые",
                Description: "Колодки тормозные для дисковых тормозов, комплект 4 шт.",
                Sku: "B-2-PAD-REAR",
                Price: 1500m,
                Currency: "RUB",
                CategoryId: Guid.NewGuid(),
                IsActive: true,
                ImageUrls: new[]
                {
                    "http://localhost:9000/catalog-images/brake-pad-rear-1.jpg"
                },
                Standard: "ISO 9001",
                SalesUnit: 1, // Штуки/Комплекты
                SalesStep: 1m,
                Attributes: new Dictionary<string, string>
                {
                    { "Материал", "Керамика" },
                    { "Ось", "Задняя" },
                    { "Комплектация", "Комплект на 2 колеса" }
                },
                PriceTiers: new List<PriceTierDto>
                {
                    new(10, 1400m, "RUB"),
                    new(50, 1250m, "RUB")
                }));

            await SeedMongoProductAsync(new ProductReadDto(
                Id: Guid.NewGuid(),
                Name: "Фильтр масляный двигателя",
                Description: "Высокоэффективный фильтр очистки моторного масла",
                Sku: "F-1-OIL-FILTER",
                Price: 500m,
                Currency: "RUB",
                CategoryId: Guid.NewGuid(),
                IsActive: true,
                ImageUrls: new[]
                {
                    "http://localhost:9000/catalog-images/oil-filter-01.jpg",
                    "http://localhost:9000/catalog-images/oil-filter-02.jpg"
                },
                Standard: "SAE J1858",
                SalesUnit: 1,
                SalesStep: 10m,
                Attributes: new Dictionary<string, string>
                {
                    { "Тип фильтра", "Навинчиваемый" },
                    { "Совместимость", "Универсальный" },
                    { "Высота", "120 мм" }
                },
                PriceTiers: new List<PriceTierDto>
                {
                    new(20, 450m, "RUB"),
                    new(100, 400m, "RUB")
                }));

            // Act
            var response = await _client.GetAsync(
                "/api/products/public/search?term=%D0%9A%D0%BE%D0%BB%D0%BE%D0%B4%D0%BA%D0%B8&page=1",
                TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var results = await response.Content.ReadFromJsonAsync<IReadOnlyList<ProductReadDto>>(
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
