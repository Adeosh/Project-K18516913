using FluentAssertions;
using Krepim.Identity.Application.Features.Profile.UpdateUserProfile;
using Krepim.Identity.Domain.Aggregates;
using Krepim.Identity.Domain.Enums;
using Krepim.Identity.Domain.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using NSubstitute;

namespace Krepim.Identity.Application.Tests.Features.Profile
{
    public class UpdateUserProfileCommandHandlerTests
    {
        private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly UpdateUserProfileCommandHandler _handler;

        public UpdateUserProfileCommandHandlerTests() =>
            _handler = new UpdateUserProfileCommandHandler(_userRepository, _unitOfWork);

        [Fact]
        public async Task Handle_Should_ReturnConflict_WhenEmailIsTakenByOtherUser()
        {
            var user = User.Create("old@mail.com", "hash", Role.Client).Value;
            _userRepository.GetByIdAsync(user.Id, TestContext.Current.CancellationToken).Returns(user);
            _userRepository.IsEmailUniqueAsync("taken@mail.com", TestContext.Current.CancellationToken).Returns(false);

            var cmd = new UpdateUserProfileCommand(user.Id, "taken@mail.com", null, null);
            var result = await _handler.Handle(cmd, TestContext.Current.CancellationToken);

            result.Error.Code.Should().Be("User.EmailNotUnique");
        }

        [Fact]
        public async Task Handle_Should_UpdateProfile_WhenDataIsValid()
        {
            var user = User.Create("old@mail.com", "hash", Role.Client).Value;
            _userRepository.GetByIdAsync(user.Id, TestContext.Current.CancellationToken).Returns(user);
            _userRepository.IsEmailUniqueAsync("new@mail.com", TestContext.Current.CancellationToken).Returns(true);

            var cmd = new UpdateUserProfileCommand(user.Id, "new@mail.com", "8999", null);
            await _handler.Handle(cmd, TestContext.Current.CancellationToken);

            user.Email.Should().Be("new@mail.com");
            await _unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
        }
    }
}
