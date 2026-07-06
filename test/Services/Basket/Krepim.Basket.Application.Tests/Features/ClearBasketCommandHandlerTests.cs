using FluentAssertions;
using Krepim.Basket.Application.Features.ClearBasket;
using Krepim.Basket.Domain.Interfaces;
using NSubstitute;

namespace Krepim.Basket.Application.Tests.Features
{
    public class ClearBasketCommandHandlerTests
    {
        private readonly IBasketRepository _repository;
        private readonly ClearBasketCommandHandler _handler;

        public ClearBasketCommandHandlerTests()
        {
            _repository = Substitute.For<IBasketRepository>();
            _handler = new ClearBasketCommandHandler(_repository);
        }

        [Fact]
        public async Task Handle_Should_CallDeleteBasketAsync_AndReturnSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new ClearBasketCommand(userId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            await _repository.Received(1).DeleteBasketAsync(userId, Arg.Any<CancellationToken>());
        }
    }
}
