using FluentAssertions;
using Krepim.Catalog.Application.Features.UpdateProduct;
using Krepim.Catalog.Application.Models.DTOs;
using Krepim.Catalog.Domain.Enums;
using Krepim.Catalog.IntegrationTests.Infrastructure;
using Krepim.Testing.Shared.Authentication;
using MassTransit.Testing;
using System.Net;
using System.Net.Http.Json;

namespace Krepim.Catalog.IntegrationTests.Features.Products
{
    public class ManagerEndpointsTests : IClassFixture<CatalogWebApplicationFactory>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly ProductTestHelper _productHelper;

        public ManagerEndpointsTests(CatalogWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);

            var harness = factory.Services.GetTestHarness();
            _productHelper = new ProductTestHelper(_client, harness);
        }

        public void Dispose()
        {
            _productHelper.CleanupCreatedProductsAsync().GetAwaiter().GetResult();
            _client.Dispose();
        }

        [Fact]
        public async Task UpdateProduct_Should_ReturnNoContent_WhenDataIsValid()
        {
            // Arrange
            var productId = await _productHelper.CreateTestProductAsync(
                "Товар для обновления",
                $"UPD-{Guid.NewGuid()}",
                isActive: true);

            var updateCommand = new UpdateProductCommand(
                productId,
                Name: "Обновленная заклепка М6х15",
                Description: "Новое описание",
                CategoryId: Guid.Parse("4d6804ce-708d-4fd3-994f-641d65ebbae5"),
                ImageUrls: new[] { "http://localhost:9000/new-image.jpg" },
                Standard: "DIN 7338",
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 250m,
                Attributes: new Dictionary<string, string> { { "Материал", "Сталь" } },
                PriceTiers: new List<PriceTierDto> { new(1000, 3.8m, "RUB") }
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
            var productId = await _productHelper.CreateTestProductAsync(
                "Товар для публикации",
                $"PUB-{Guid.NewGuid()}",
                isActive: false);

            // Act
            var response = await _client.PostAsync($"/api/products/manager/{productId}/publish", null, TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteProduct_Should_ReturnNoContent_WhenProductExists()
        {
            // Arrange
            var productId = await _productHelper.CreateTestProductAsync(
                "Товар для удаления",
                $"DEL-{Guid.NewGuid()}",
                isActive: true);

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
                Name: "Фантомный товар",
                Description: "Его нет в базе",
                CategoryId: Guid.Parse("4d6804ce-708d-4fd3-994f-641d65ebbae5"),
                ImageUrls: new[] { "http://localhost:9000/phantom.jpg" },
                Standard: "DIN 7338",
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 250m,
                Attributes: new Dictionary<string, string>(),
                PriceTiers: new List<PriceTierDto>()
            );

            // Act
            var response = await _client.PutAsJsonAsync($"/api/products/manager/{nonExistentId}", updateCommand, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UploadImages_Should_ReturnOk_WhenFilesAreValid()
        {
            // Arrange
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(new byte[] { 0x1, 0x2, 0x3 });
            content.Add(fileContent, "files", "test-image.jpg");

            // Act
            var response = await _client.PostAsync("/api/products/manager/images", content, TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<UploadResponse>(cancellationToken: TestContext.Current.CancellationToken);
            result.Should().NotBeNull();
            result!.Urls.Should().NotBeEmpty();
        }
    }
}
