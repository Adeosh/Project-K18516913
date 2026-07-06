using FluentAssertions;
using Krepim.EventBus.Events.Inventory;
using Krepim.Inventory.Application.Features.CreditStock;
using Krepim.Inventory.Application.Models.DTOs;
using Krepim.Inventory.Domain.Entities;
using Krepim.Inventory.Infrastructure.Database;
using Krepim.Inventory.IntegrationTests.Infrastructure;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Krepim.Inventory.IntegrationTests.Features
{
    public class InventoryIntegrationTests : IClassFixture<InventoryWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly ITestHarness _testHarness;
        private readonly IServiceScopeFactory _scopeFactory;

        public InventoryIntegrationTests(InventoryWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _testHarness = factory.Services.GetTestHarness();
            _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
        }

        [Fact]
        public async Task OrderCreatedEvent_Should_ReserveStock_And_ApiShould_ReturnCorrectAvailableQuantity()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                var stock = StockItem.Create(productId, 100).Value;
                db.StockItems.Add(stock);
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var integrationEvent = new OrderCreatedIntegrationEvent(
                orderId,
                1500m,
                new List<OrderItemPayload> { new(productId, 30) });

            // Act
            await _testHarness.Bus.Publish(integrationEvent, TestContext.Current.CancellationToken);

            var consumerHarness = _testHarness.GetConsumerHarness<Application.Consumers.OrderCreatedEventConsumer>();
            var consumed = await consumerHarness.Consumed.Any<OrderCreatedIntegrationEvent>(TestContext.Current.CancellationToken);

            consumed.Should().BeTrue();

            // Act 2
            var response = await _client.GetAsync($"/api/inventory/{productId}", TestContext.Current.CancellationToken);
            var errorBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            response.IsSuccessStatusCode.Should().BeTrue($"Эндпоинт упал с ошибкой: {errorBody}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var stockDto = await response.Content.ReadFromJsonAsync<StockDto>(cancellationToken: TestContext.Current.CancellationToken);

            // Assert 2
            stockDto.Should().NotBeNull();
            stockDto!.AvailableQuantity.Should().Be(70);
        }

        [Fact]
        public async Task OrderCreatedEvent_Should_FailToReserve_When_InsufficientStock()
        {
            // Arrange
            var productId = Guid.NewGuid();
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                db.StockItems.Add(StockItem.Create(productId, 10).Value);
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var integrationEvent = new OrderCreatedIntegrationEvent(
                Guid.NewGuid(), 1000m, new List<OrderItemPayload> { new(productId, 50) });

            // Act
            await _testHarness.Bus.Publish(integrationEvent, TestContext.Current.CancellationToken);

            // Assert
            var consumerHarness = _testHarness.GetConsumerHarness<Application.Consumers.OrderCreatedEventConsumer>();
            (await consumerHarness.Consumed.Any<OrderCreatedIntegrationEvent>(TestContext.Current.CancellationToken)).Should().BeTrue();

            var response = await _client.GetAsync($"/api/inventory/{productId}", TestContext.Current.CancellationToken);
            var stockDto = await response.Content.ReadFromJsonAsync<StockDto>(cancellationToken: TestContext.Current.CancellationToken);
            stockDto!.AvailableQuantity.Should().Be(10);
        }

        [Fact]
        public async Task CreditStock_Should_IncreaseAvailableQuantity()
        {
            // Arrange
            var productId = Guid.NewGuid();
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                db.StockItems.Add(StockItem.Create(productId, 50).Value);
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var command = new CreditStockCommand(productId, 50);

            // Act
            var response = await _client.PostAsJsonAsync("/api/inventory/credit", command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/inventory/{productId}", TestContext.Current.CancellationToken);
            var stockDto = await getResponse.Content.ReadFromJsonAsync<StockDto>(cancellationToken: TestContext.Current.CancellationToken);
            stockDto!.AvailableQuantity.Should().Be(100);
        }
    }
}
