using FluentAssertions;
using Krepim.Catalog.Application.Features.CreateProduct;
using Krepim.Catalog.Application.Features.UpdateProduct;
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
            var command = new CreateProductCommand("Тестовый товар", "Описание", $"SKU-{Guid.NewGuid().ToString()[..6]}", 1000m, Guid.NewGuid());
            var response = await _client.PostAsJsonAsync("/api/products", command);
            return await response.Content.ReadFromJsonAsync<Guid>();
        }

        [Fact]
        public async Task UpdateProduct_Should_ReturnNoContent_WhenDataIsValid()
        {
            // Arrange
            var productId = await CreateTestProductAsync();
            var updateCommand = new UpdateProductCommand(productId, "Новое имя", "Новое описание", Guid.NewGuid());

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
            var updateCommand = new UpdateProductCommand(nonExistentId, "Name", "Desc", Guid.NewGuid());

            // Act
            var response = await _client.PutAsJsonAsync($"/api/products/manager/{nonExistentId}", updateCommand, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
