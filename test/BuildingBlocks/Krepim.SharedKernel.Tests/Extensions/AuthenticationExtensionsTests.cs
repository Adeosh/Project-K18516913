using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Krepim.SharedKernel.Extensions;

namespace Krepim.SharedKernel.Tests.Extensions
{
    public class AuthenticationExtensionsTests
    {
        [Fact]
        public void AddKrepimJwtAuth_Should_RegisterServices_When_ConfigurationIsValid()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddLogging();

            var inMemoryConfig = new Dictionary<string, string?>
            {
                {"Jwt:SecretKey", "SuperMegaStrongSecretKeyForSigningKrepimTokens12345!"},
                {"Jwt:Issuer", "Krepim.Identity"},
                {"Jwt:Audience", "Krepim.Gateway"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemoryConfig)
                .Build();

            // Act
            services.AddKrepimJwtAuth(configuration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetService<Microsoft.AspNetCore.Authentication.IAuthenticationService>().Should().NotBeNull();
            serviceProvider.GetService<Microsoft.AspNetCore.Authorization.IAuthorizationService>().Should().NotBeNull();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AddKrepimJwtAuth_Should_ThrowInvalidOperationException_When_JwtConfigIsInvalid(bool createEmptySection)
        {
            // Arrange
            var services = new ServiceCollection();
            var inMemoryConfig = new Dictionary<string, string?>();

            if (createEmptySection)
            {
                inMemoryConfig.Add("Jwt:SecretKey", " ");
                inMemoryConfig.Add("Jwt:Issuer", "Test");
            }

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemoryConfig)
                .Build();

            // Act
            Action act = () => services.AddKrepimJwtAuth(configuration);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Настройки JWT не найдены в конфигурации (секция 'Jwt').");
        }
    }
}
