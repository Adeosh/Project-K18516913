using Krepim.Mobile.Exceptions;
using Krepim.Mobile.Features.Auth.Models.DTOs;
using Krepim.Mobile.Features.Auth.Models.Enums;
using Krepim.Mobile.Features.Auth.Models.Exchange;
using Krepim.Mobile.Features.Profile.Models.DTOs;
using Krepim.Mobile.Features.Profile.Models.Exchange;
using Krepim.Mobile.Http;
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

            await Shell.Current.GoToAsync("//home");
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

                string id = "";
                if (payload.TryGetProperty("sub", out var subProp))
                    id = subProp.GetString() ?? "";
                else if (payload.TryGetProperty("nameid", out var nameidProp))
                    id = nameidProp.GetString() ?? "";

                string email = payload.TryGetProperty("email", out var emailProp)
                    ? emailProp.GetString() ?? ""
                    : "";

                long exp = payload.TryGetProperty("exp", out var expProp)
                    ? expProp.GetInt64()
                    : 0;

                string roleStr = "";
                if (payload.TryGetProperty("role", out var shortRole))
                {
                    roleStr = shortRole.GetString() ?? "";
                }
                else if (payload.TryGetProperty("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var longRole))
                {
                    roleStr = longRole.GetString() ?? "";
                }

                var role = Enum.TryParse<UserRole>(roleStr, out var parsedRole) ? parsedRole : UserRole.Client;

                return new UserClaimsDto
                {
                    Id = id,
                    Email = email,
                    Exp = exp,
                    Role = role
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ОШИБКА РАСШИФРОВКИ JWT: {ex.Message}");
                return null;
            }
        }

        public async Task<UserProfileDto> GetProfileAsync()
        {
            var response = await _httpClient.GetAsync("api/identity/profile");

            if (!response.IsSuccessStatusCode)
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                throw new ApiException(problem ?? new ProblemDetails { Status = (int)response.StatusCode });
            }

            return await response.Content.ReadFromJsonAsync<UserProfileDto>()
                   ?? throw new Exception("Не удалось прочитать профиль");
        }

        public async Task UpdateProfileAsync(UpdateProfilePayload payload)
        {
            var response = await _httpClient.PutAsJsonAsync("api/identity/profile", payload);

            if (!response.IsSuccessStatusCode)
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                throw new ApiException(problem ?? new ProblemDetails { Status = (int)response.StatusCode });
            }
        }

        public async Task ChangePasswordAsync(string oldPassword, string newPassword)
        {
            var response = await _httpClient.PostAsJsonAsync("api/identity/profile/change-password", new
            {
                oldPassword,
                newPassword
            });

            if (!response.IsSuccessStatusCode)
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                throw new ApiException(problem ?? new ProblemDetails { Status = (int)response.StatusCode });
            }
        }
    }
}
