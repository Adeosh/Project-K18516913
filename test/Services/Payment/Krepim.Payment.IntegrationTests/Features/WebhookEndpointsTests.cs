using FluentAssertions;
using Krepim.Payment.Domain.Entities;
using Krepim.Payment.Infrastructure.Database;
using Krepim.Payment.IntegrationTests.Infrastructure;
using Krepim.SharedKernel.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Krepim.Payment.IntegrationTests.Features
{
    public class WebhookEndpointsTests : IClassFixture<PaymentWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly PaymentDbContext _dbContext;


        public WebhookEndpointsTests(PaymentWebApplicationFactory factory)
        {
            _client = factory.CreateClient();

            var scope = factory.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        }

        [Fact]
        public async Task MockWebhook_Should_UpdateTransactionStatus_When_StatusIsSuccess()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var transaction = PaymentTransaction.Create(orderId, 1000m).Value;
            var externalId = "mock_tx_123";
            transaction.SetExternalDetails(externalId, "http://test.url");

            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var payload = new { TransactionId = externalId, Status = PaymentStatus.Succeeded };

            // Act
            var response = await _client.PostAsJsonAsync("/api/payments/webhook/mock", payload, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            _dbContext.ChangeTracker.Clear();

            var updatedTransaction = await _dbContext.Transactions.FindAsync(new object?[] { transaction.Id }, TestContext.Current.CancellationToken);
            updatedTransaction.Should().NotBeNull();
            updatedTransaction!.Status.Should().Be(PaymentStatus.Succeeded);
        }

        [Fact]
        public async Task MockWebhook_Should_ReturnBadRequest_When_TransactionDoesNotExist()
        {
            // Arrange
            var payload = new { TransactionId = "non_existent_id", Status = PaymentStatus.Succeeded };

            // Act
            var response = await _client.PostAsJsonAsync("/api/payments/webhook/mock", payload, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData(PaymentStatus.Succeeded, HttpStatusCode.OK)]
        [InlineData(PaymentStatus.Failed, HttpStatusCode.OK)]
        [InlineData(PaymentStatus.Refunded, HttpStatusCode.BadRequest)]
        public async Task MockWebhook_Should_HandleDifferentStatuses(PaymentStatus status, HttpStatusCode expectedCode)
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var transaction = PaymentTransaction.Create(orderId, 1000m).Value;
            var externalId = $"tx_{status}";
            transaction.SetExternalDetails(externalId, "http://test.url");

            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            _dbContext.ChangeTracker.Clear();

            var payload = new { TransactionId = externalId, Status = status };

            // Act
            var response = await _client.PostAsJsonAsync("/api/payments/webhook/mock", payload, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(expectedCode);

            var updatedTransaction = await _dbContext.Transactions.FindAsync(new object?[] { transaction.Id }, TestContext.Current.CancellationToken);
            if (expectedCode == HttpStatusCode.OK)
            {
                updatedTransaction!.Status.Should().Be(status);
            }
        }
    }
}
