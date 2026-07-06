using FluentAssertions;
using Krepim.Identity.Application.Features.Profile.GetUserProfile;
using Krepim.Identity.Domain.Aggregates;
using Krepim.Identity.Domain.Enums;
using Krepim.Identity.Domain.Interfaces;
using NSubstitute;

namespace Krepim.Identity.Application.Tests.Features.Profile
{
    public class GetUserProfileQueryHandlerTests
    {
        private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
        private readonly GetUserProfileQueryHandler _handler;

        public GetUserProfileQueryHandlerTests() => _handler = new GetUserProfileQueryHandler(_userRepository);

        [Fact]
        public async Task Handle_Should_ReturnProfile_WhenUserExists()
        {
            var user = User.Create("test@mail.com", "hash", Role.Client, "123").Value;
            _userRepository.GetByIdAsync(user.Id, TestContext.Current.CancellationToken).Returns(user);

            var result = await _handler.Handle(new GetUserProfileQuery(user.Id), TestContext.Current.CancellationToken);

            result.IsSuccess.Should().BeTrue();
            result.Value.Email.Should().Be("test@mail.com");
        }
    }
}
