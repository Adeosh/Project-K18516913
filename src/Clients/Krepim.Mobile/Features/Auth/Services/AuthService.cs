using Krepim.Mobile.Features.Auth.Models.DTOs;
using Krepim.Mobile.Features.Auth.Models.Enums;
using Krepim.Mobile.Features.Auth.Models.Exchange;
using System.Net.Http.Json;
using System.Text.Json;

namespace Krepim.Mobile.Features.Auth.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public UserClaimsDto? CurrentUser { get; private set; }
        public bool IsAuthenticated => CurrentUser != null;

        public AuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task InitializeAsync()
        {
            var token = await SecureStorage.Default.GetAsync("krepim_token");
            if (!string.IsNullOrEmpty(token))
            {
                var claims = DecodeJwt(token);
                if (claims != null && DateTimeOffset.FromUnixTimeSeconds(claims.Exp) > DateTimeOffset.UtcNow)
                    CurrentUser = claims;
                else
                    await LogoutAsync();
            }
        }

        public async Task LoginAsync(string email, string password)
        {
            var request = new LoginRequest(email, password);
            var response = await _httpClient.PostAsJsonAsync("/api/identity/login", request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            string? token = result.ValueKind == JsonValueKind.String
                ? result.GetString()
                : result.GetProperty("token").GetString();

            if (string.IsNullOrEmpty(token))
                throw new Exception("Токен не получен от сервера.");

            await SecureStorage.Default.SetAsync("krepim_token", token);
            CurrentUser = DecodeJwt(token);
        }

        public async Task RegisterAsync(string email, string password, string? phoneNumber)
        {
            var request = new RegisterRequest(email, password, phoneNumber);
            var response = await _httpClient.PostAsJsonAsync("/api/identity/register", request);

            response.EnsureSuccessStatusCode();

            await LoginAsync(email, password);
        }

        public async Task LogoutAsync()
        {
            SecureStorage.Default.Remove("krepim_token");
            CurrentUser = null;
            await Shell.Current.GoToAsync("//login");
        }

        private UserClaimsDto? DecodeJwt(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length < 2) return null;

                var base64 = parts[1].PadRight(parts[1].Length + (4 - parts[1].Length % 4) % 4, '=')
                                     .Replace('-', '+').Replace('_', '/');

                var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                var payload = JsonSerializer.Deserialize<JsonElement>(json);

                return new UserClaimsDto
                {
                    Id = payload.TryGetProperty("nameid", out var id) ? id.GetString() ?? "" : payload.GetProperty("sub").GetString() ?? "",
                    Email = payload.GetProperty("email").GetString() ?? "",
                    Exp = payload.GetProperty("exp").GetInt64(),
                    Role = Enum.TryParse<UserRole>(payload.GetProperty("role").GetString(), out var role) ? role : UserRole.Client
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
