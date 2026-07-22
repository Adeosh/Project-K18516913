using Krepim.Mobile.Exceptions;
using Krepim.Mobile.Features.Ordering.Models.DTOs;
using Krepim.Mobile.Http;
using System.Net.Http.Json;

namespace Krepim.Mobile.Features.Ordering.Services
{
    public class OrderService
    {
        private readonly HttpClient _httpClient;

        public OrderService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<List<OrderDto>> GetMyOrdersAsync()
        {
            var response = await _httpClient.GetAsync("api/orders");
            if (!response.IsSuccessStatusCode)
                throw new ApiException(await response.Content.ReadFromJsonAsync<ProblemDetails>() ?? new ProblemDetails { Status = (int)response.StatusCode });

            return await response.Content.ReadFromJsonAsync<List<OrderDto>>() ?? new();
        }

        public async Task<OrderDto> GetByIdAsync(string id)
        {
            var response = await _httpClient.GetAsync($"api/orders/{id}");
            if (!response.IsSuccessStatusCode)
                throw new ApiException(await response.Content.ReadFromJsonAsync<ProblemDetails>() ?? new ProblemDetails { Status = (int)response.StatusCode });

            return await response.Content.ReadFromJsonAsync<OrderDto>() ?? throw new Exception("Заказ не найден");
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            var response = await _httpClient.GetAsync("api/orders/all");
            if (!response.IsSuccessStatusCode)
                throw new ApiException(await response.Content.ReadFromJsonAsync<ProblemDetails>() ?? new ProblemDetails { Status = (int)response.StatusCode });

            return await response.Content.ReadFromJsonAsync<List<OrderDto>>() ?? new();
        }
    }
}
