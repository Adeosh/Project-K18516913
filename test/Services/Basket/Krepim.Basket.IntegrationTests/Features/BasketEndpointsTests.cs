using FluentAssertions;
using Krepim.Basket.Application.Models.Exchange;
using Krepim.Basket.Domain.Entities;
using Krepim.Basket.IntegrationTests.Infrastructure;
using Krepim.Testing.Shared.Authentication;
using System.Net;
using System.Net.Http.Json;

namespace Krepim.Basket.IntegrationTests.Features
{
    public class BasketEndpointsTests : IClassFixture<BasketWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public BasketEndpointsTests(BasketWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme, "dummy");
        }

        [Fact]
        public async Task GetBasket_Should_ReturnEmptyBasket_WhenUserIsNew()
        {
            // Act
            var response = await _client.GetAsync("/api/basket", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var basket = await response.Content.ReadFromJsonAsync<CustomerBasket>(cancellationToken: TestContext.Current.CancellationToken);

            basket.Should().NotBeNull();
            basket!.Items.Should().BeEmpty();
            basket.TotalPrice.Should().Be(0);
            basket.UserId.ToString().Should().Be(TestAuthHandler.DefaultUserId);
        }

        [Fact]
        public async Task AddItemToBasket_Should_ReturnBasketWithItem_And_CalculateTotal()
        {
            // Arrange
            var request = new AddItemRequest(Guid.NewGuid(), "Масло", "OIL-1", 1500m, 2);

            // Act
            var response = await _client.PostAsJsonAsync("/api/basket/items", request, TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var basket = await response.Content.ReadFromJsonAsync<CustomerBasket>(cancellationToken: TestContext.Current.CancellationToken);

            basket.Should().NotBeNull();
            basket!.Items.Should().ContainSingle();
            basket.Items[0].ProductName.Should().Be("Масло");
            basket.Items[0].Quantity.Should().Be(2);
            basket.TotalPrice.Should().Be(3000m);
        }

        [Fact]
        public async Task AddItemToBasket_Should_ReturnBadRequest_WhenQuantityIsInvalid()
        {
            // Arrange
            var request = new AddItemRequest(Guid.NewGuid(), "Брак", "BAD", 100m, -1);

            // Act
            var response = await _client.PostAsJsonAsync("/api/basket/items", request, TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ClearBasket_Should_RemoveAllItems()
        {
            // Arrange
            var request = new AddItemRequest(Guid.NewGuid(), "Свеча", "SP-1", 500m, 4);
            await _client.PostAsJsonAsync("/api/basket/items", request, TestContext.Current.CancellationToken);

            // Act
            var deleteResponse = await _client.DeleteAsync("/api/basket", TestContext.Current.CancellationToken);

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync("/api/basket", TestContext.Current.CancellationToken);
            var basket = await getResponse.Content.ReadFromJsonAsync<CustomerBasket>(cancellationToken: TestContext.Current.CancellationToken);

            basket!.Items.Should().BeEmpty();
        }
    }
}
