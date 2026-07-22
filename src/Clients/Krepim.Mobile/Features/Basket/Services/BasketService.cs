using Krepim.Mobile.Exceptions;
using Krepim.Mobile.Features.Basket.Models.DTOs;
using Krepim.Mobile.Features.Basket.Models.Exchange;
using Krepim.Mobile.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Krepim.Mobile.Features.Basket.Services
{
    public class BasketService
    {
        private readonly HttpClient _httpClient;

        public BasketService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<CustomerBasketDto> GetBasketAsync()
        {
            var response = await _httpClient.GetAsync("api/basket");

            if (!response.IsSuccessStatusCode)
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                throw new ApiException(problem ?? new ProblemDetails { Status = (int)response.StatusCode });
            }

            return await response.Content.ReadFromJsonAsync<CustomerBasketDto>()
                   ?? throw new Exception("Не удалось прочитать корзину");
        }

        public async Task UpdateQuantityAsync(string productId, int quantity, decimal price)
        {
            var payload = new { quantity, price };
            var response = await _httpClient.PutAsJsonAsync($"api/basket/items/{productId}", payload);

            if (!response.IsSuccessStatusCode)
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                throw new ApiException(problem ?? new ProblemDetails { Status = (int)response.StatusCode });
            }
        }

        public async Task AddItemAsync(AddBasketItemRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/basket/items", request);

            if (!response.IsSuccessStatusCode)
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                throw new ApiException(problem ?? new ProblemDetails { Status = (int)response.StatusCode });
            }
        }

        public async Task RemoveItemAsync(string productId)
        {
            var response = await _httpClient.DeleteAsync($"api/basket/{productId}");

            if (!response.IsSuccessStatusCode)
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                throw new ApiException(problem ?? new ProblemDetails { Status = (int)response.StatusCode });
            }
        }

        public async Task<string> CheckoutAsync(CheckoutRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/basket/checkout", request);

            if (!response.IsSuccessStatusCode)
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                throw new ApiException(problem ?? new ProblemDetails { Status = (int)response.StatusCode });
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (result.TryGetProperty("orderId", out var orderIdProp))
            {
                return orderIdProp.GetString() ?? throw new Exception("OrderId пуст");
            }

            throw new Exception("Не удалось получить OrderId");
        }
    }
}
