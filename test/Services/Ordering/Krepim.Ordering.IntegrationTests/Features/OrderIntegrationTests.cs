using FluentAssertions;
using Krepim.EventBus.Events.Basket;
using Krepim.Ordering.Application.Models.DTOs;
using Krepim.Ordering.IntegrationTests.Infrastructure;
using Krepim.Testing.Shared.Authentication;
using MassTransit.Testing;
using System.Net;
using System.Net.Http.Json;

namespace Krepim.Ordering.IntegrationTests.Features
{
    public class OrderIntegrationTests : IClassFixture<OrderingWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly ITestHarness _testHarness;

        public OrderIntegrationTests(OrderingWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme, "dummy");

            _testHarness = factory.Services.GetTestHarness();
        }

        [Fact]
        public async Task CheckoutEvent_Should_CreateOrder_And_ApiShould_ReturnItToUser()
        {
            // Arrange
            var userId = Guid.Parse(TestAuthHandler.DefaultUserId);
            var productId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            var checkoutItems = new List<BasketCheckoutItem>
            {
                new(productId, 250.00m, 3)
            };

            var integrationEvent = new BasketCheckoutIntegrationEvent(
                orderId,
                userId,
                "test@example.com",
                "+7 (999) 123-45-67",
                750.00m,
                "Казань, ул.Баумана",
                10000.00,
                10434.00,
                "12",
                checkoutItems);

            // Act 1
            await _testHarness.Bus.Publish(integrationEvent, TestContext.Current.CancellationToken);

            var consumerHarness = _testHarness.GetConsumerHarness<Application.Consumers.BasketCheckoutEventConsumer>();
            var consumed = await consumerHarness.Consumed.Any<BasketCheckoutIntegrationEvent>(TestContext.Current.CancellationToken);
            consumed.Should().BeTrue();

            await Task.Delay(200, TestContext.Current.CancellationToken);

            // Act 2
            var response = await _client.GetAsync("/api/orders", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>(cancellationToken: TestContext.Current.CancellationToken);

            orders.Should().NotBeNull();

            var order = orders!.FirstOrDefault(x => x.Id == orderId);
            order.Should().NotBeNull("Заказ с ID {0} не найден в списке", orderId);

            order!.Status.Should().Be("Pending");
            order.Status.Should().Be("Pending");
            order.TotalPrice.Should().Be(750.00m);
            order.FullAddress.Should().Be("Казань, ул.Баумана");
            order.Latitude.Should().Be(10000.00);
            order.Longitude.Should().Be(10434.00);
            order.Flat.Should().Be("12");

            order.Items.Should().ContainSingle();
            order.Items.First().ProductId.Should().Be(productId);
            order.Items.First().UnitPrice.Should().Be(250.00m);
            order.Items.First().Quantity.Should().Be(3);
        }

        [Fact]
        public async Task GetOrderById_Should_ReturnOrder_When_OrderExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.Parse(TestAuthHandler.DefaultUserId);
            var integrationEvent = new BasketCheckoutIntegrationEvent(
                orderId,
                userId,
                "test@example.com",
                "+7 (999) 123-45-67",
                100m, 
                "Адрес", 
                0, 
                0, 
                "1",
                new List<BasketCheckoutItem>());

            await _testHarness.Bus.Publish(integrationEvent, TestContext.Current.CancellationToken);

            var consumerHarness = _testHarness.GetConsumerHarness<Application.Consumers.BasketCheckoutEventConsumer>();
            await consumerHarness.Consumed.Any<BasketCheckoutIntegrationEvent>(TestContext.Current.CancellationToken);

            // Act
            var response = await _client.GetAsync($"/api/orders/{orderId}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var order = await response.Content.ReadFromJsonAsync<OrderDto>(cancellationToken: TestContext.Current.CancellationToken);

            order.Should().NotBeNull();
            order!.Id.Should().Be(orderId);
        }

        [Fact]
        public async Task GetOrderById_Should_ReturnNotFound_When_OrderDoesNotExist()
        {
            // Act
            var randomOrderId = Guid.NewGuid();
            var response = await _client.GetAsync($"/api/orders/{randomOrderId}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
