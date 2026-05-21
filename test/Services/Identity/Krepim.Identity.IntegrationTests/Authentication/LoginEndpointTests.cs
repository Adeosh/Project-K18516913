using FluentAssertions;
using Krepim.Identity.Application.Features.Authentication;
using Krepim.Identity.Application.Features.Registration;
using Krepim.Identity.Domain.Enums;
using Krepim.Identity.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Krepim.Identity.IntegrationTests.Authentication
{
    [Collection("IdentityTests")]
    public class LoginEndpointTests : IClassFixture<IdentityWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public LoginEndpointTests(IdentityWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_Should_ReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var email = "login_success@krepim.pro";
            var password = "ValidPassword123!";
            var registerCommand = new RegisterCommand(email, password, Role.Client);
            await _client.PostAsJsonAsync("/api/users/register", registerCommand, cancellationToken: TestContext.Current.CancellationToken);

            var loginCommand = new LoginCommand(email, password);

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/login", loginCommand, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var token = await response.Content.ReadFromJsonAsync<string>(cancellationToken: TestContext.Current.CancellationToken);
            token.Should().NotBeNullOrWhiteSpace();

            var tokenParts = token!.Split('.');
            tokenParts.Length.Should().Be(3);
        }

        [Fact]
        public async Task Login_Should_ReturnBadRequest_WhenPasswordIsIncorrect()
        {
            // Arrange
            var email = "login_fail@krepim.pro";
            var correctPassword = "ValidPassword123!";
            var wrongPassword = "WrongPassword123!";

            await _client.PostAsJsonAsync("/api/users/register", new RegisterCommand(email, correctPassword, Role.Client), cancellationToken: TestContext.Current.CancellationToken);

            var loginCommand = new LoginCommand(email, wrongPassword);

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/login", loginCommand, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
