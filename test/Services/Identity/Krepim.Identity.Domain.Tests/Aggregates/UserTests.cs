using FluentAssertions;
using Krepim.Identity.Domain.Aggregates;
using Krepim.Identity.Domain.Enums;
using Krepim.SharedKernel.Results;


namespace Krepim.Identity.Domain.Tests.Aggregates
{
    public class UserTests
    {
        [Fact]
        public void Create_Should_ReturnSuccess_WhenEmailAndRoleAreValid()
        {
            // Arrange
            var email = "client@krepim.pro";
            var passwordHash = "hashed_string";
            var role = Role.Client;

            // Act
            var result = User.Create(email, passwordHash, role);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Email.Should().Be(email);
            result.Value.PasswordHash.Should().Be(passwordHash);
            result.Value.Role.Should().Be(role);
            result.Value.Id.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Create_Should_ReturnFailure_WhenEmailIsNullOrWhiteSpace(string invalidEmail)
        {
            // Act
            var result = User.Create(invalidEmail, "hash", Role.Client);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("User.InvalidEmail");
            result.Error.Type.Should().Be(ErrorType.Validation);
        }
    }
}
