using FluentAssertions;
using Krepim.EventBus.Events.Identity;
using Krepim.Identity.Application.Features.Registration;
using Krepim.Identity.Application.Interfaces;
using Krepim.Identity.Domain.Aggregates;
using Krepim.Identity.Domain.Enums;
using Krepim.Identity.Domain.Errors;
using Krepim.Identity.Domain.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using MassTransit;
using NSubstitute;

namespace Krepim.Identity.Application.Tests.Features.Registration
{
    public class RegisterCommandHandlerTests
    {
        private readonly IUserRepository _userRepositoryMock;
        private readonly IPasswordHasher _passwordHasherMock;
        private readonly IPublishEndpoint _publishEndpointMock;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly RegisterCommandHandler _handler;

        public RegisterCommandHandlerTests()
        {
            _userRepositoryMock = Substitute.For<IUserRepository>();
            _passwordHasherMock = Substitute.For<IPasswordHasher>();
            _publishEndpointMock = Substitute.For<IPublishEndpoint>();
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();

            _handler = new RegisterCommandHandler(
                _userRepositoryMock,
                _passwordHasherMock,
                _publishEndpointMock,
                _unitOfWorkMock);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenEmailIsNotUnique()
        {
            // Arrange
            var command = new RegisterCommand("existing@mail.com", "Password123", Role.Client, null);
            _userRepositoryMock.IsEmailUniqueAsync(command.Email, Arg.Any<CancellationToken>())
                .Returns(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(IdentityErrors.EmailNotUnique);
            await _userRepositoryMock.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_RegisterUser_WithCorrectData_WhenRequestIsValid()
        {
            // Arrange
            var command = new RegisterCommand("new@mail.com", "Password123", Role.Client, "89001112233");
            _userRepositoryMock.IsEmailUniqueAsync(command.Email, Arg.Any<CancellationToken>())
                .Returns(true);
            _passwordHasherMock.Hash(command.Password).Returns("hashed_pass");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();

            await _userRepositoryMock.Received(1).AddAsync(
                Arg.Is<User>(u =>
                    u.Email == command.Email &&
                    u.Role == command.Role &&
                    u.PhoneNumber == command.PhoneNumber),
                Arg.Any<CancellationToken>());

            await _publishEndpointMock.Received(1).Publish(
                Arg.Is<UserRegisteredIntegrationEvent>(e => e.Role == command.Role.ToString()),
                Arg.Any<CancellationToken>());

            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
