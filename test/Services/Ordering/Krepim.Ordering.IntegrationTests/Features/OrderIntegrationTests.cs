using FluentAssertions;
using Krepim.EventBus.Events.Basket;
using Krepim.Ordering.Application.Models;
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
            _testHarness = factory.Services.GetTestHarness();
            _client.DefaultRequestHeaders.Remove("X-Test-Anonymous");
        }

        [Fact]
        public async Task CheckoutEvent_Should_CreateOrder_And_ApiShould_ReturnItToUser()
        {
            // Arrange
            var userId = Guid.Parse(TestAuthHandler.DefaultUserId);
            var productId = Guid.NewGuid();

            var checkoutItems = new List<BasketCheckoutItem>
            {
                new(productId, 250.00m, 3)
            };

            var integrationEvent = new BasketCheckoutIntegrationEvent(
                userId,
                750.00m,
                "Казань",
                "Баумана",
                "420000",
                checkoutItems);

            // Act 1
            await _testHarness.Bus.Publish(integrationEvent, TestContext.Current.CancellationToken);

            var consumerHarness = _testHarness.GetConsumerHarness<Application.Consumers.BasketCheckoutEventConsumer>();
            var consumed = await consumerHarness.Consumed.Any<BasketCheckoutIntegrationEvent>(TestContext.Current.CancellationToken);
            consumed.Should().BeTrue();

            // Микро-пауза для фиксации транзакции в Postgres контейнере
            await Task.Delay(200, TestContext.Current.CancellationToken);

            // Act 2
            var response = await _client.GetAsync("/api/orders", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var orders = await response.Content.ReadFromJsonAsync<List<OrderModel>>(cancellationToken: TestContext.Current.CancellationToken);

            orders.Should().NotBeNull();
            orders.Should().ContainSingle();

            var order = orders!.First();
            order.Status.Should().Be("Pending");
            order.TotalPrice.Should().Be(750.00m);
            order.City.Should().Be("Казань");
            order.Street.Should().Be("Баумана");
            order.ZipCode.Should().Be("420000");

            order.Items.Should().ContainSingle();
            order.Items.First().ProductId.Should().Be(productId);
            order.Items.First().UnitPrice.Should().Be(250.00m);
            order.Items.First().Quantity.Should().Be(3);
        }
    }
}
