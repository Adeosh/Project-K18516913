using Krepim.Mobile.Features.Catalog.Models.DTOs;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace Krepim.Mobile.Features.Catalog.Services
{
    public class CatalogService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _apiHost;

        public CatalogService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var apiUrl = configuration["ApiGatewayUrl"] ?? "http://127.0.0.1:5078/";
            _apiHost = new Uri(apiUrl).Host;
        }

        private void FixImageUrls(ProductDto? product)
        {
            if (product?.ImageUrls == null) return;

            for (int i = 0; i < product.ImageUrls.Count; i++)
            {
                var url = product.ImageUrls[i];

                try
                {
                    var uri = new Uri(url);

                    if (uri.Host is "localhost" or "127.0.0.1" or "minio" or "host.docker.internal")
                    {
                        var builder = new UriBuilder(uri)
                        {
                            Host = _apiHost
                        };
                        product.ImageUrls[i] = builder.ToString();
                    }
                }
                catch{ }
            }
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/products/categories");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadFromJsonAsync<JsonElement>();

                if (json.ValueKind == JsonValueKind.Object && json.TryGetProperty("value", out var valueProp))
                    json = valueProp;

                return json.Deserialize<List<CategoryDto>>(_jsonOptions) ?? new();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки категорий: {ex.Message}");
                return new List<CategoryDto>();
            }
        }

        public async Task<PagedList<ProductDto>> SearchAsync(string? searchTerm, Guid? categoryId, int page = 1, int pageSize = 8)
        {
            try
            {
                var queryParams = new List<string> { $"page={page}", $"pageSize={pageSize}" };

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    queryParams.Add($"term={Uri.EscapeDataString(searchTerm)}");

                if (categoryId.HasValue && categoryId.Value != Guid.Empty)
                    queryParams.Add($"categoryId={categoryId}");

                var url = $"/api/products/public/search?{string.Join("&", queryParams)}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadFromJsonAsync<JsonElement>();

                if (json.ValueKind == JsonValueKind.Object && json.TryGetProperty("value", out var valueProp))
                    json = valueProp;

                if (json.ValueKind == JsonValueKind.Array)
                {
                    var items = json.Deserialize<List<ProductDto>>(_jsonOptions) ?? new();
                    return new PagedList<ProductDto> { Items = items, TotalCount = items.Count, Page = page, PageSize = pageSize };
                }

                var result = json.Deserialize<PagedList<ProductDto>>(_jsonOptions);

                if (result?.Items != null)
                {
                    foreach (var item in result.Items)
                    {
                        FixImageUrls(item);
                    }
                }
                return result ?? new PagedList<ProductDto> { Items = new() };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка поиска товаров: {ex.Message}");
                return new PagedList<ProductDto> { Items = new() };
            }
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/products/public/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadFromJsonAsync<JsonElement>();

                if (json.ValueKind == JsonValueKind.Object && json.TryGetProperty("value", out var valueProp))
                    json = valueProp;

                var product = json.Deserialize<ProductDto>(_jsonOptions);
                FixImageUrls(product);
                return product;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки карточки: {ex.Message}");
                return null;
            }
        }
    }
}
