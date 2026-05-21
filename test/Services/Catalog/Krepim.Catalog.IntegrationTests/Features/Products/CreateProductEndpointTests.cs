using FluentAssertions;
using Krepim.Catalog.Application.Features.CreateProduct;
using Krepim.Catalog.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Krepim.Catalog.IntegrationTests.Features.Products
{
    public class CreateProductEndpointTests : IClassFixture<CatalogWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public CreateProductEndpointTests(CatalogWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Clear();
        }

        [Fact]
        public async Task CreateProduct_Should_ReturnCreated_WhenUserIsManagerAndDataIsValid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Manager");

            var command = new CreateProductCommand(
                "Свеча зажигания NGK",
                "Иридиевая",
                "NGK-IRIDIUM",
                1200m,
                Guid.NewGuid());

            // Act
            var response = await _client.PostAsJsonAsync("/api/products", command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var productId = await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: TestContext.Current.CancellationToken);
            productId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CreateProduct_Should_ReturnForbidden_WhenUserIsAnOrdinaryClient()
        {
            // Arrange
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Client");

            var command = new CreateProductCommand("Свеча", "Desc", "Code", 100m, Guid.NewGuid());

            // Act
            var response = await _client.PostAsJsonAsync("/api/products", command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateProduct_Should_ReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _client.DefaultRequestHeaders.Add("X-Test-Anonymous", "true");

            var command = new CreateProductCommand("Свеча", "Desc", "Code", 100m, Guid.NewGuid());

            // Act
            var response = await _client.PostAsJsonAsync("/api/products", command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateProduct_Should_ReturnBadRequest_WhenPriceIsInvalid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Manager");
            var command = new CreateProductCommand("Свеча", "Desc", "NGK-123", -500m, Guid.NewGuid());

            // Act
            var response = await _client.PostAsJsonAsync("/api/products", command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
