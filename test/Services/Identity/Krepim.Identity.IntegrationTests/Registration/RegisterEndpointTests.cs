using FluentAssertions;
using Krepim.Identity.Application.Features.Registration;
using Krepim.Identity.Domain.Enums;
using Krepim.Identity.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Krepim.Identity.IntegrationTests.Registration
{
    public class RegisterEndpointTests : IClassFixture<IdentityWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public RegisterEndpointTests(IdentityWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_Should_ReturnOkAndCreateUser_WhenDataIsValid()
        {
            // Arrange
            var command = new RegisterCommand("integration@krepim.pro", "StrongPass123!", Role.Client, "+7 (999) 000-00-00");

            // Act
            var response = await _client.PostAsJsonAsync("/api/identity/register", command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var userId = await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: TestContext.Current.CancellationToken);
            userId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Register_Should_ReturnConflict_WhenEmailAlreadyExists()
        {
            // Arrange
            var command = new RegisterCommand("duplicate@krepim.pro", "Pass123!", Role.Client, "+7 (999) 000-00-00");

            await _client.PostAsJsonAsync("/api/identity/register", command, cancellationToken: TestContext.Current.CancellationToken);

            // Act
            var duplicateResponse = await _client.PostAsJsonAsync("/api/identity/register", command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
    }
}
