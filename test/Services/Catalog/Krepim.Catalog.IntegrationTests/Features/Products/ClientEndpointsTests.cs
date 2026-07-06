using FluentAssertions;
using Krepim.Catalog.Application.Models.DTOs;
using Krepim.Catalog.Domain.Enums;
using Krepim.Catalog.IntegrationTests.Helpers;
using Krepim.Catalog.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Krepim.Catalog.IntegrationTests.Features.Products
{
    public class ClientEndpointsTests : IClassFixture<CatalogWebApplicationFactory>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly ProductTestHelper _productHelper;

        public ClientEndpointsTests(CatalogWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Clear();
            _productHelper = new ProductTestHelper(_client);
        }

        public void Dispose()
        {
            _productHelper.CleanupCreatedProducts().GetAwaiter().GetResult();
            _client.Dispose();
        }

        [Fact]
        public async Task GetProductById_Should_ReturnProduct_WhenProductExistsAndIsActive()
        {
            // Arrange
            var productId = await _productHelper.CreateTestProduct("Тестовый продукт", "TEST-001", true);
            await _productHelper.ClearHeaders();

            // Act
            var product = await _productHelper.GetProductByIdAndGetResult(productId);

            // Assert
            product.Should().NotBeNull();
            product!.Id.Should().Be(productId);
            product.Name.Should().Be("Тестовый продукт");
            product.Sku.Should().Be("TEST-001");
            product.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetProductById_Should_ReturnNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            await _productHelper.ClearHeaders();
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _productHelper.GetProductById(nonExistentId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetProductById_Should_ReturnNotFound_WhenProductIsInactive()
        {
            // Arrange
            var productId = await _productHelper.CreateTestProduct("Неактивный продукт", "TEST-INACTIVE", false);
            await _productHelper.ClearHeaders();

            // Act
            var response = await _productHelper.GetProductById(productId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetProductById_Should_ReturnNotFound_WhenProductIsDeleted()
        {
            // Arrange
            var productId = await _productHelper.CreateTestProduct("Удаленный продукт", "TEST-DELETED", true);
            await _productHelper.DeleteProduct(productId);
            await _productHelper.ClearHeaders();

            // Act
            var response = await _productHelper.GetProductById(productId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task SearchProducts_Should_ReturnMatchingProducts_WhenSearchTermMatches()
        {
            // Arrange
            await _productHelper.CreateMultipleProducts(
                ("Заклепка резьбовая М6", "ZRM-M6-001", true),
                ("Заклепка резьбовая М8", "ZRM-M8-002", true),
                ("Болт высокопрочный", "BOLT-001", true)
            );
            await _productHelper.ClearHeaders();

            // Act
            var results = await _productHelper.SearchProductsAndGetResult("заклепка");

            // Assert
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(p => p.Name.Should().Contain("Заклепка"));
        }

        [Fact]
        public async Task SearchProducts_Should_ReturnEmptyList_WhenSearchTermDoesNotMatch()
        {
            // Arrange
            await _productHelper.CreateTestProduct("Заклепка резьбовая", "ZRM-001", true);
            await _productHelper.ClearHeaders();

            // Act
            var results = await _productHelper.SearchProductsAndGetResult("несуществующий");

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchProducts_Should_ReturnOnlyActiveProducts_WhenOnlyActiveIsTrue()
        {
            // Arrange
            await _productHelper.CreateMultipleProducts(
                ("Активный продукт", "ACTIVE-001", true),
                ("Неактивный продукт", "INACTIVE-001", false)
            );
            await _productHelper.ClearHeaders();

            // Act
            var results = await _productHelper.SearchProductsAndGetResult("продукт");

            // Assert
            results.Should().HaveCount(1);
            results.Should().AllSatisfy(p => p.IsActive.Should().BeTrue());
            results.Should().Contain(p => p.Name == "Активный продукт");
            results.Should().NotContain(p => p.Name == "Неактивный продукт");
        }

        [Fact]
        public async Task SearchProducts_Should_ReturnOk_WhenNoParametersProvided()
        {
            // Arrange
            await _productHelper.CreateMultipleProducts(
                ("Продукт 1", "PROD-1", true),
                ("Продукт 2", "PROD-2", true)
            );
            await _productHelper.ClearHeaders();

            // Act
            var response = await _client.GetAsync("/public/search", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var results = await response.Content.ReadFromJsonAsync<List<ProductReadDto>>(cancellationToken: TestContext.Current.CancellationToken);
            results.Should().NotBeNull();
            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task SearchProducts_Should_SearchCaseInsensitive()
        {
            // Arrange
            await _productHelper.CreateTestProduct("Заклепка резьбовая", "ZRM-001", true);
            await _productHelper.ClearHeaders();

            // Act
            var resultsLower = await _productHelper.SearchProductsAndGetResult("заклепка");
            var resultsUpper = await _productHelper.SearchProductsAndGetResult("ЗАКЛЕПКА");

            // Assert
            resultsLower.Should().HaveCount(1);
            resultsUpper.Should().HaveCount(1);
            resultsLower.First().Id.Should().Be(resultsUpper.First().Id);
        }

        [Fact]
        public async Task SearchProducts_Should_SearchByPartialName()
        {
            // Arrange
            await _productHelper.CreateTestProduct("Заклепка резьбовая М6", "ZRM-001", true);
            await _productHelper.ClearHeaders();

            // Act
            var results = await _productHelper.SearchProductsAndGetResult("резьбовая");

            // Assert
            results.Should().HaveCount(1);
            results.First().Name.Should().Be("Заклепка резьбовая М6");
        }

        [Fact]
        public async Task SearchProducts_Should_ReturnMatchingProductsBySku()
        {
            // Arrange
            await _productHelper.CreateMultipleProducts(
                ("Заклепка", "ZRM-M6-001", true),
                ("Болт", "BOLT-001", true)
            );
            await _productHelper.ClearHeaders();

            // Act
            var results = await _productHelper.SearchProductsAndGetResult("ZRM");

            // Assert
            results.Should().HaveCount(1);
            results.First().Sku.Should().Be("ZRM-M6-001");
        }

        [Fact]
        public async Task GetProductById_Should_ReturnProductWithAllFields_WhenProductHasFullData()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var attributes = new Dictionary<string, string>
            {
                { "Длина", "100 мм" },
                { "Вес", "0.5 кг" }
            };
            var priceTiers = new List<PriceTierDto>
            {
                new(50, 9.0m, "RUB"),
                new(100, 8.0m, "RUB")
            };
            var imageUrls = new[] { "http://localhost:9000/image1.jpg", "http://localhost:9000/image2.jpg" };

            // Создаем продукт через API
            var productId = await _productHelper.CreateTestProduct(
                name: "Полный продукт",
                sku: "FULL-001",
                isActive: true,
                price: 15.0m,
                imageUrls: imageUrls
            );
            await _productHelper.ClearHeaders();

            // Act
            var product = await _productHelper.GetProductByIdAndGetResult(productId);

            // Assert
            product.Should().NotBeNull();
            product!.Id.Should().Be(productId);
            product.Name.Should().Be("Полный продукт");
            product.Sku.Should().Be("FULL-001");
            product.Price.Should().Be(15.0m);
            product.IsActive.Should().BeTrue();
            product.ImageUrls.Should().HaveCount(2);
        }
    }
}