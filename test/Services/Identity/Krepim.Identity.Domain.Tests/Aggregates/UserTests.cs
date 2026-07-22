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
        public void Create_Should_ReturnFailure_WhenEmailIsNullOrWhiteSpace(string? invalidEmail)
        {
            // Act
            var result = User.Create(invalidEmail!, "hash", Role.Client);

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

        [Fact]
        public void Create_Should_NotValidatePasswordHash_When_PasswordHashIsNull()
        {
            // Act
            var result = User.Create("test@email.com", null!, Role.Client);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.PasswordHash.Should().BeNull();
        }

        [Fact]
        public void Create_Should_NotValidatePasswordHash_When_PasswordHashIsEmpty()
        {
            // Act
            var result = User.Create("test@email.com", "", Role.Client);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.PasswordHash.Should().BeEmpty();
        }

        [Fact]
        public void Create_Should_ReturnSuccess_WithPhoneNumber_WhenValid()
        {
            // Arrange
            var email = "client@krepim.pro";
            var passwordHash = "hashed_string";
            var role = Role.Client;
            var phoneNumber = "89001234567";

            // Act
            var result = User.Create(email, passwordHash, role, phoneNumber);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.PhoneNumber.Should().Be(phoneNumber);
        }

        [Fact]
        public void Create_Should_ReturnSuccess_WithNullPhoneNumber_WhenValid()
        {
            // Act
            var result = User.Create("client@krepim.pro", "hash", Role.Client, null);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.PhoneNumber.Should().BeNull();
        }

        [Fact]
        public void Create_Should_GenerateNewId_EachTime()
        {
            // Act
            var user1 = User.Create("user1@email.com", "hash", Role.Client).Value;
            var user2 = User.Create("user2@email.com", "hash", Role.Client).Value;

            // Assert
            user1.Id.Should().NotBe(user2.Id);
        }

        [Fact]
        public void UpdateProfile_Should_ReturnSuccess_When_EmailIsValid()
        {
            // Arrange
            var user = User.Create("old@email.com", "hash", Role.Client).Value;
            var newEmail = "new@email.com";

            // Act
            var result = user.UpdateProfile(newEmail, null, null);

            // Assert
            result.IsSuccess.Should().BeTrue();
            user.Email.Should().Be(newEmail);
        }

        [Fact]
        public void UpdateProfile_Should_ReturnFailure_When_EmailIsEmpty()
        {
            // Arrange
            var user = User.Create("old@email.com", "hash", Role.Client).Value;

            // Act
            var result = user.UpdateProfile("", null, null);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("User.InvalidEmail");
            result.Error.Type.Should().Be(ErrorType.Validation);
            user.Email.Should().Be("old@email.com");
        }

        [Fact]
        public void UpdateProfile_Should_UpdatePhoneAndAddress_WhenProvided()
        {
            // Arrange
            var user = User.Create("email@email.com", "hash", Role.Client).Value;
            var newPhone = "89991234567";
            var newAddress = new Krepim.SharedKernel.ValueObjects.Address("Казань", 55.0, 37.0, "1");

            // Act
            var result = user.UpdateProfile("email@email.com", newPhone, newAddress);

            // Assert
            result.IsSuccess.Should().BeTrue();
            user.PhoneNumber.Should().Be(newPhone);
            user.DefaultAddress.Should().Be(newAddress);
        }

        [Fact]
        public void UpdateProfile_Should_ClearPhoneAndAddress_WhenNull()
        {
            // Arrange
            var user = User.Create("email@email.com", "hash", Role.Client, "89001234567").Value;
            user.UpdateProfile("email@email.com", "89001234567", new Krepim.SharedKernel.ValueObjects.Address("Москва", 55.0, 37.0, "1"));

            // Act
            var result = user.UpdateProfile("email@email.com", null, null);

            // Assert
            result.IsSuccess.Should().BeTrue();
            user.PhoneNumber.Should().BeNull();
            user.DefaultAddress.Should().BeNull();
        }

        [Fact]
        public void UpdatePasswordHash_Should_UpdateHash_When_Valid()
        {
            // Arrange
            var user = User.Create("test@email.com", "old_hash", Role.Client).Value;
            var newHash = "new_hashed_string";

            // Act
            user.UpdatePasswordHash(newHash);

            // Assert
            user.PasswordHash.Should().Be(newHash);
        }

        [Fact]
        public void UpdatePasswordHash_Should_RaiseDomainEvent_When_Called()
        {
            // Arrange
            var user = User.Create("test@email.com", "old_hash", Role.Client).Value;
            var newHash = "new_hashed_string";

            // Act
            user.UpdatePasswordHash(newHash);

            // Assert
            user.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<UserPasswordChangedDomainEvent>();
        }

        [Fact]
        public void UpdatePasswordHash_Should_RaiseDomainEventWithCorrectUserId()
        {
            // Arrange
            var user = User.Create("test@email.com", "old_hash", Role.Client).Value;
            var newHash = "new_hashed_string";

            // Act
            user.UpdatePasswordHash(newHash);

            // Assert
            var domainEvent = (UserPasswordChangedDomainEvent)user.DomainEvents.First();
            domainEvent.UserId.Should().Be(user.Id);
        }

        [Fact]
        public void UpdatePasswordHash_Should_AlwaysRaiseEvent_When_Called()
        {
            // Arrange
            var user = User.Create("test@email.com", "hash", Role.Client).Value;
            user.ClearDomainEvents();

            // Act
            user.UpdatePasswordHash("hash");

            // Assert
            user.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<UserPasswordChangedDomainEvent>();
        }

        [Fact]
        public void UpdatePasswordHash_Should_UpdateHashAndRaiseEvent_EvenWithSameValue()
        {
            // Arrange
            var user = User.Create("test@email.com", "old_hash", Role.Client).Value;
            user.ClearDomainEvents();

            // Act
            user.UpdatePasswordHash("old_hash");

            // Assert
            user.PasswordHash.Should().Be("old_hash");
            user.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<UserPasswordChangedDomainEvent>();
        }
    }
}
