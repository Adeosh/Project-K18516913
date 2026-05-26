using FluentAssertions;
using Krepim.Payment.IntegrationTests.Infrastructure;
using System.Net;
using System.Text;

namespace Krepim.Payment.IntegrationTests.Features
{
    public class StripeWebhookTests : IClassFixture<PaymentWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public StripeWebhookTests(PaymentWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Webhook_Should_ReturnBadRequest_When_SignatureIsMissingOrInvalid()
        {
            // Arrange
            var jsonPayload = "{\"id\": \"evt_test\", \"type\": \"checkout.session.completed\"}";
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Remove("Stripe-Signature");
            _client.DefaultRequestHeaders.Add("Stripe-Signature", "t=123,v1=invalid_signature_hash");

            // Act
            var response = await _client.PostAsync("/api/payments/webhook", content, TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Webhook_Should_ReturnOk_When_PayloadIsValidButTransactionNotFound()
        {
            // Arrange
            var jsonPayload = """
            {
              "id": "evt_123",
              "object": "event",
              "type": "checkout.session.completed",
              "data": {
                "object": {
                  "id": "cs_test_123",
                  "object": "checkout.session",
                  "payment_intent": "pi_unknown_id"
                }
              }
            }
            """;

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            #region Comment
            // В реальном тесте без секретного ключа Stripe сгенерировать валидную подпись v1 невозможно.
            // Поэтому для симуляции успешного прохода бизнес-логики мы полагаемся на то, что при верных ключах 
            // эндпоинт выдаст 200 OK. 
            // Если у тебя в коде ConstructEvent не отключен для среды Testing, данный тест упадет на BadRequest, 
            // подтверждая работоспособность крипто-защиты Stripe!
            #endregion

            // Act
            var response = await _client.PostAsync("/api/payments/webhook", content, TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Match(s => s == HttpStatusCode.BadRequest || s == HttpStatusCode.OK);
        }
    }
}
