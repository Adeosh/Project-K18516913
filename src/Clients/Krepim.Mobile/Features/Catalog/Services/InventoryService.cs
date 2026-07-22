using Krepim.Mobile.Features.Catalog.Models.DTOs;
using System.Net.Http.Json;

namespace Krepim.Mobile.Features.Catalog.Services
{
    public class InventoryService
    {
        private readonly HttpClient _httpClient;

        public InventoryService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<int> GetStockAsync(Guid productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/inventory/{productId}");

                if (!response.IsSuccessStatusCode)
                    return 0;

                var stockDto = await response.Content.ReadFromJsonAsync<StockDto>();
                return stockDto?.AvailableQuantity ?? 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при получении остатков: {ex.Message}");
                return 0;
            }
        }
    }
}
