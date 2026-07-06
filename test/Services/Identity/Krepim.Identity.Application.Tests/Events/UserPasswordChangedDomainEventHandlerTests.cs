using Krepim.Identity.Application.Events;
using Krepim.Identity.Domain.Events;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;

namespace Krepim.Identity.Application.Tests.Events
{
    public class UserPasswordChangedDomainEventHandlerTests
    {
        private readonly IDistributedCache _cacheMock;
        private readonly UserPasswordChangedDomainEventHandler _handler;

        public UserPasswordChangedDomainEventHandlerTests()
        {
            _cacheMock = Substitute.For<IDistributedCache>();
            _handler = new UserPasswordChangedDomainEventHandler(_cacheMock);
        }

        [Fact]
        public async Task Handle_Should_SetCacheEntry_When_EventIsReceived()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notification = new UserPasswordChangedDomainEvent(userId);
            string expectedKey = $"jwt-blocklist:{userId}";

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            await _cacheMock.Received(1).SetAsync(
                expectedKey,
                Arg.Any<byte[]>(),
                Arg.Any<DistributedCacheEntryOptions>(),
                Arg.Any<CancellationToken>()
            );
        }
    }
}
