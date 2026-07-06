using FluentAssertions;
using Krepim.Identity.Application.Features.Profile.ChangePassword;
using Krepim.Identity.Application.Interfaces;
using Krepim.Identity.Domain.Aggregates;
using Krepim.Identity.Domain.Enums;
using Krepim.Identity.Domain.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using NSubstitute;

namespace Krepim.Identity.Application.Tests.Features.Profile
{
    public class ChangePasswordCommandHandlerTests
    {
        private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
        private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly ChangePasswordCommandHandler _handler;

        public ChangePasswordCommandHandlerTests() =>
            _handler = new ChangePasswordCommandHandler(_userRepository, _passwordHasher, _unitOfWork);

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenUserNotFound()
        {
            _userRepository.GetByIdAsync(Arg.Any<Guid>(), TestContext.Current.CancellationToken).Returns((User?)null);
            var result = await _handler.Handle(new ChangePasswordCommand(Guid.NewGuid(), "old", "new"), TestContext.Current.CancellationToken);
            result.Error.Code.Should().Be("User.NotFound");
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenOldPasswordIsInvalid()
        {
            var user = User.Create("e@mail.com", "hash", Role.Client).Value;
            _userRepository.GetByIdAsync(user.Id, TestContext.Current.CancellationToken).Returns(user);
            _passwordHasher.Verify("wrong", user.PasswordHash).Returns(false);

            var result = await _handler.Handle(new ChangePasswordCommand(user.Id, "wrong", "new"), TestContext.Current.CancellationToken);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("User.InvalidPassword");
        }

        [Fact]
        public async Task Handle_Should_UpdatePassword_WhenValid()
        {
            var user = User.Create("e@mail.com", "old_hash", Role.Client).Value;
            _userRepository.GetByIdAsync(user.Id, TestContext.Current.CancellationToken).Returns(user);
            _passwordHasher.Verify("old", user.PasswordHash).Returns(true);
            _passwordHasher.Hash("new").Returns("new_hash");

            await _handler.Handle(new ChangePasswordCommand(user.Id, "old", "new"), TestContext.Current.CancellationToken);

            user.PasswordHash.Should().Be("new_hash");
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
