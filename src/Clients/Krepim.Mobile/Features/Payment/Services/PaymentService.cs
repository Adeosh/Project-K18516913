using Krepim.Mobile.Exceptions;
using Krepim.Mobile.Features.Payment.Models.Exchange;
using Krepim.Mobile.Http;
using System.Net.Http.Json;

namespace Krepim.Mobile.Features.Payment.Services
{
    public class PaymentService
    {
        private readonly HttpClient _httpClient;

        public PaymentService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<string> GetPaymentUrlAsync(string orderId)
        {
            var response = await _httpClient.GetAsync($"api/payments/{orderId}/url");

            if (!response.IsSuccessStatusCode)
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                throw new ApiException(problem ?? new ProblemDetails { Status = (int)response.StatusCode });
            }

            var rawString = await response.Content.ReadAsStringAsync();
            return rawString.Trim('"');
        }

        public async Task SimulateMockPaymentAsync(MockWebhookRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/payments/webhook/mock", request);

            if (!response.IsSuccessStatusCode)
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                throw new ApiException(problem ?? new ProblemDetails { Status = (int)response.StatusCode });
            }
        }
    }
}
