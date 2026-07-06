using FluentAssertions;
using Krepim.Identity.Domain.Aggregates;
using Krepim.Identity.Domain.Enums;
using Krepim.Identity.Domain.Events;
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

        [Fact]
        public void UpdateProfile_Should_UpdateUserFields_WhenDataIsValid()
        {
            // Arrange
            var user = User.Create("old@email.com", "hash", Role.Client).Value;
            var newEmail = "new@email.com";
            var newPhone = "89001234567";
            var newAddress = new Krepim.SharedKernel.ValueObjects.Address("Москва", 55.0, 37.0, "1");

            // Act
            user.UpdateProfile(newEmail, newPhone, newAddress);

            // Assert
            user.Email.Should().Be(newEmail);
            user.PhoneNumber.Should().Be(newPhone);
            user.DefaultAddress.Should().Be(newAddress);
        }

        [Fact]
        public void UpdatePasswordHash_Should_UpdatePasswordAndRaiseDomainEvent()
        {
            // Arrange
            var user = User.Create("test@email.com", "old_hash", Role.Client).Value;
            var newHash = "new_hashed_string";

            // Act
            user.UpdatePasswordHash(newHash);

            // Assert
            user.PasswordHash.Should().Be(newHash);

            user.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<UserPasswordChangedDomainEvent>();

            var domainEvent = (UserPasswordChangedDomainEvent)user.DomainEvents.First();
            domainEvent.UserId.Should().Be(user.Id);
        }
    }
}
