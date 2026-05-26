using FluentAssertions;
using Krepim.Identity.Application.Features.Authentication;
using Krepim.Identity.Application.Interfaces;
using Krepim.Identity.Domain.Aggregates;
using Krepim.Identity.Domain.Enums;
using Krepim.Identity.Domain.Errors;
using Krepim.Identity.Domain.Interfaces;
using NSubstitute;

namespace Krepim.Identity.Application.Tests.Features.Authentication
{
    public class LoginCommandHandlerTests
    {
        private readonly IUserRepository _userRepositoryMock;
        private readonly IPasswordHasher _passwordHasherMock;
        private readonly IJwtProvider _jwtProviderMock;
        private readonly LoginCommandHandler _handler;

        public LoginCommandHandlerTests()
        {
            _userRepositoryMock = Substitute.For<IUserRepository>();
            _passwordHasherMock = Substitute.For<IPasswordHasher>();
            _jwtProviderMock = Substitute.For<IJwtProvider>();

            _handler = new LoginCommandHandler(
                _userRepositoryMock,
                _passwordHasherMock,
                _jwtProviderMock);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenUserDoesNotExist()
        {
            // Arrange
            var command = new LoginCommand("wrong@mail.com", "Password123!");

            _userRepositoryMock.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
                .Returns((User?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(IdentityErrors.InvalidCredentials);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenPasswordIsIncorrect()
        {
            // Arrange
            var command = new LoginCommand("user@mail.com", "WrongPassword!");
            var user = User.Create(command.Email, "hash", Role.Client).Value;

            _userRepositoryMock.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
                .Returns(user);

            _passwordHasherMock.Verify(command.Password, user.PasswordHash).Returns(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(IdentityErrors.InvalidCredentials);
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccessAndToken_WhenCredentialsAreValid()
        {
            // Arrange
            var command = new LoginCommand("user@mail.com", "CorrectPassword!");
            var user = User.Create(command.Email, "hash", Role.Client).Value;
            var expectedToken = "jwt_token_string";

            _userRepositoryMock.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
                .Returns(user);

            _passwordHasherMock.Verify(command.Password, user.PasswordHash).Returns(true);

            _jwtProviderMock.Generate(user).Returns(expectedToken);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expectedToken);
        }
    }
}
