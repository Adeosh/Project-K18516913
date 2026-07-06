using FluentAssertions;
using Krepim.Identity.Application.Features.Authentication;
using Krepim.Identity.Application.Features.Registration;
using Krepim.Identity.Application.Models.Exchange;
using Krepim.Identity.Domain.Enums;
using Krepim.Identity.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Krepim.Identity.IntegrationTests.Profile
{
    [Collection("IdentityTests")]
    public class ProfileEndpointTests : IClassFixture<IdentityWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ProfileEndpointTests(IdentityWebApplicationFactory factory)
        {
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.Configure<AuthenticationOptions>(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    });

                    services.PostConfigureAll<JwtBearerOptions>(options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters.ValidateIssuer = false;
                        options.TokenValidationParameters.ValidateAudience = false;

                        var testSecretKey = "SUPER_SECRET_KEY_FOR_INTEGRATION_TESTS_KREPIM_123!";
                        options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(testSecretKey));
                    });
                });
            }).CreateClient();
        }

        private async Task<string> AuthenticateUserAsync(string email, string password)
        {
            var regCommand = new RegisterCommand(email, password, Role.Client, "+7 (999) 000-00-00");
            var regRes = await _client.PostAsJsonAsync("/api/identity/register", regCommand);
            regRes.EnsureSuccessStatusCode();

            var loginCommand = new LoginCommand(email, password);
            var loginResponse = await _client.PostAsJsonAsync("/api/identity/login", loginCommand);
            loginResponse.EnsureSuccessStatusCode();

            var token = await loginResponse.Content.ReadFromJsonAsync<string>();
            return token ?? string.Empty;
        }

        private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string token, object? body = null)
        {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (body != null)
                request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            return request;
        }

        [Fact]
        public async Task GetProfile_Should_ReturnProfile_WhenTokenIsValid()
        {
            // Arrange
            var email = "get_profile@krepim.pro";
            var token = await AuthenticateUserAsync(email, "Password123!");
            var request = CreateAuthorizedRequest(HttpMethod.Get, "/api/identity/profile", token);

            // Act
            var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);
            var errorBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            // Assert
            response.IsSuccessStatusCode.Should().BeTrue($"Ожидался успех, но API вернул {response.StatusCode}. Детали ошибки: {errorBody}");

            var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>(cancellationToken: TestContext.Current.CancellationToken);
            profile.Should().NotBeNull();
            profile!.Email.Should().Be(email);
        }

        [Fact]
        public async Task GetProfile_Should_ReturnUnauthorized_WhenTokenIsMissing()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/identity/profile");

            // Act
            var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateProfile_Should_ReturnNoContent_WhenDataIsValid()
        {
            // Arrange
            var email = "update_profile@krepim.pro";
            var token = await AuthenticateUserAsync(email, "Password123!");

            var updateRequest = new UpdateProfileRequest(
                email,
                "+7 (999) 123-45-67",
                null);

            var request = CreateAuthorizedRequest(HttpMethod.Put, "/api/identity/profile", token, updateRequest);

            // Act
            var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task ChangePassword_Should_ReturnOk_WhenDataIsValid()
        {
            // Arrange
            var email = "change_pass@krepim.pro";
            var oldPassword = "Password123!";
            var newPassword = "NewStrongPassword123!";

            var token = await AuthenticateUserAsync(email, oldPassword);

            var changePassRequest = new ChangePasswordRequest(oldPassword, newPassword);
            var request = CreateAuthorizedRequest(HttpMethod.Post, "/api/identity/profile/change-password", token, changePassRequest);

            // Act
            var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task ChangePassword_Should_ReturnBadRequest_WhenNewPasswordIsTooShort()
        {
            // Arrange
            var email = "short_pass@krepim.pro";
            var token = await AuthenticateUserAsync(email, "Password123!");

            var changePassRequest = new ChangePasswordRequest("Password123!", "123");
            var request = CreateAuthorizedRequest(HttpMethod.Post, "/api/identity/profile/change-password", token, changePassRequest);

            // Act
            var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);
            var errorBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            errorBody.Should().Contain("Password.TooShort");
        }
    }
}
