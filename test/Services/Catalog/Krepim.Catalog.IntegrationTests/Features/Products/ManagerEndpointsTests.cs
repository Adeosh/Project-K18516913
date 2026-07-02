using FluentAssertions;
using Krepim.Catalog.Application.Features.CreateProduct;
using Krepim.Catalog.Application.Features.UpdateProduct;
using Krepim.Catalog.Application.Models.DTOs;
using Krepim.Catalog.Domain.Enums;
using Krepim.Catalog.IntegrationTests.Infrastructure;
using Krepim.Testing.Shared.Authentication;
using System.Net;
using System.Net.Http.Json;

namespace Krepim.Catalog.IntegrationTests.Features.Products
{
    public class ManagerEndpointsTests : IClassFixture<CatalogWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ManagerEndpointsTests(CatalogWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        }

        private async Task<Guid> CreateTestProductAsync()
        {
            var command = new CreateProductCommand(
                Name: "Заклепка резьбовая цилиндрическая с малым фланцем М6х15",
                Description: "Заклепка резьбовая цилиндрическая с малым фланцем",
                Sku: "ZRM-M6X15-ST-ZN",
                Price: 4.50m,
                CategoryId: Guid.Parse("4d6804ce-708d-4fd3-994f-641d65ebbae5"),
                ImageUrls: new[]
                {
                    "http://localhost:9000/catalog-images/0780d3b0-b469-44f2-9ca0-25645206f1c0-7492748982.jpeg",
                    "http://localhost:9000/catalog-images/2e3733dd-d760-483f-9970-c266b854f9c1-7492748983.jpeg"
                },
                Standard: "DIN 7338",
                SalesUnit: SalesUnit.Pcs,
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
                }
            );
            var response = await _client.PostAsJsonAsync("/api/products", command);
            return await response.Content.ReadFromJsonAsync<Guid>();
        }

        [Fact]
        public async Task UpdateProduct_Should_ReturnNoContent_WhenDataIsValid()
        {
            // Arrange
            var productId = await CreateTestProductAsync();
            var updateCommand = new UpdateProductCommand(
                productId,
                Name: "Заклепка резьбовая цилиндрическая с малым фланцем М6х15",
                Description: "Заклепка резьбовая цилиндрическая с малым фланцем",
                CategoryId: Guid.Parse("4d6804ce-708d-4fd3-994f-641d65ebbae5"),
                ImageUrls: new[]
                {
                    "http://localhost:9000/catalog-images/0780d3b0-b469-44f2-9ca0-25645206f1c0-7492748982.jpeg",
                    "http://localhost:9000/catalog-images/2e3733dd-d760-483f-9970-c266b854f9c1-7492748983.jpeg"
                },
                Standard: "DIN 7338",
                SalesUnit: SalesUnit.Pcs,
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
                }
            );

            // Act
            var response = await _client.PutAsJsonAsync($"/api/products/manager/{productId}", updateCommand, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task PublishProduct_Should_ReturnNoContent_WhenProductExists()
        {
            // Arrange
            var productId = await CreateTestProductAsync();

            // Act
            var response = await _client.PostAsync($"/api/products/manager/{productId}/publish", null, TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteProduct_Should_ReturnNoContent_WhenProductExists()
        {
            // Arrange
            var productId = await CreateTestProductAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/products/manager/{productId}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateProduct_Should_ReturnNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updateCommand = new UpdateProductCommand(
                nonExistentId,
                Name: "Заклепка резьбовая цилиндрическая с малым фланцем М6х15",
                Description: "Заклепка резьбовая цилиндрическая с малым фланцем",
                CategoryId: Guid.Parse("4d6804ce-708d-4fd3-994f-641d65ebbae5"),
                ImageUrls: new[]
                {
                    "http://localhost:9000/catalog-images/0780d3b0-b469-44f2-9ca0-25645206f1c0-7492748982.jpeg",
                    "http://localhost:9000/catalog-images/2e3733dd-d760-483f-9970-c266b854f9c1-7492748983.jpeg"
                },
                Standard: "DIN 7338",
                SalesUnit: SalesUnit.Pcs,
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

            // Act
            var response = await _client.PutAsJsonAsync($"/api/products/manager/{nonExistentId}", updateCommand, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
