using Krepim.Payment.Application.Interfaces;
using Krepim.Payment.Application.Models.Exchange;
using System.Net.Http.Json;

namespace Krepim.Payment.Infrastructure.ExternalServices
{
    public sealed class StripePaymentService(HttpClient httpClient) : IPaymentGateway
    {
        public async Task<PaymentGatewayResponse> InitializePaymentAsync(Guid orderId, decimal amount, CancellationToken ct)
        {
            var requestBody = new { OrderId = orderId, Amount = amount, Currency = "RUB" };
            var response = await httpClient.PostAsJsonAsync("/v1/charges", requestBody, ct);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PaymentGatewayResponse>(cancellationToken: ct);
            return result ?? throw new InvalidOperationException("Пустой ответ от платежного шлюза.");
        }
    }
}
