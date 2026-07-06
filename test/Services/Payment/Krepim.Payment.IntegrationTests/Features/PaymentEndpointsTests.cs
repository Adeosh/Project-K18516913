using FluentAssertions;
using Krepim.Payment.Domain.Entities;
using Krepim.Payment.Infrastructure.Database;
using Krepim.Payment.IntegrationTests.Infrastructure;
using Krepim.Testing.Shared.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Krepim.Payment.IntegrationTests.Features
{
    public class PaymentEndpointsTests : IClassFixture<PaymentWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly PaymentDbContext _dbContext;

        public PaymentEndpointsTests(PaymentWebApplicationFactory factory)
        {
            _client = factory.CreateClient();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme, "dummy");

            var scope = factory.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        }

        [Fact]
        public async Task GetPaymentUrl_Should_ReturnUrl_When_OrderExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            var transaction = PaymentTransaction.Create(orderId, 1000m).Value;
            transaction.SetExternalDetails("mock_tx_123", "http://test.url");

            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            _dbContext.ChangeTracker.Clear();

            // Act
            var response = await _client.GetAsync($"/api/payments/{orderId}/url", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var updatedTransaction = await _dbContext.Transactions.FirstOrDefaultAsync(
                x => x.OrderId == orderId,
                TestContext.Current.CancellationToken);

            updatedTransaction.Should().NotBeNull();
            updatedTransaction!.PaymentUrl.Should().NotBeNullOrEmpty();
        }
    }
}
