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
            var command = new RegisterCommand("integration@krepim.pro", "StrongPass123!", Role.Client);

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/register", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var userId = await response.Content.ReadFromJsonAsync<Guid>();
            userId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Register_Should_ReturnConflict_WhenEmailAlreadyExists()
        {
            // Arrange
            var command = new RegisterCommand("duplicate@krepim.pro", "Pass123!", Role.Client);

            await _client.PostAsJsonAsync("/api/users/register", command);

            // Act
            var duplicateResponse = await _client.PostAsJsonAsync("/api/users/register", command);

            // Assert
            duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
    }
}
