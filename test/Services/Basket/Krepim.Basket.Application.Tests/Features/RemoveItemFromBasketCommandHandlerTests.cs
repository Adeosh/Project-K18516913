using FluentAssertions;
using Krepim.Basket.Application.Features.RemoveItem;
using Krepim.Basket.Domain.Entities;
using Krepim.Basket.Domain.Interfaces;
using NSubstitute;

namespace Krepim.Basket.Application.Tests.Features
{
    public class RemoveItemFromBasketCommandHandlerTests
    {
        private readonly IBasketRepository _repository;
        private readonly RemoveItemFromBasketCommandHandler _handler;

        public RemoveItemFromBasketCommandHandlerTests()
        {
            _repository = Substitute.For<IBasketRepository>();
            _handler = new RemoveItemFromBasketCommandHandler(_repository);
        }

        [Fact]
        public async Task Handle_Should_ReturnNewBasket_WhenBasketDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new RemoveItemFromBasketCommand(userId, productId);

            _repository.GetBasketAsync(userId, Arg.Any<CancellationToken>()).Returns((CustomerBasket?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.UserId.Should().Be(userId);
            result.Value.Items.Should().BeEmpty();

            await _repository.DidNotReceive().UpdateBasketAsync(Arg.Any<CustomerBasket>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_RemoveItemAndUpdateBasket_WhenBasketExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productIdToRemove = Guid.NewGuid();
            var basket = new CustomerBasket(userId);
            basket.AddItem(new BasketItem(productIdToRemove, "Товар 1", "SKU1", 100m, 1));
            basket.AddItem(new BasketItem(Guid.NewGuid(), "Товар 2", "SKU2", 200m, 1));

            var command = new RemoveItemFromBasketCommand(userId, productIdToRemove);

            _repository.GetBasketAsync(userId, Arg.Any<CancellationToken>()).Returns(basket);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().NotContain(i => i.ProductId == productIdToRemove);
            result.Value.Items.Should().HaveCount(1);

            await _repository.Received(1).UpdateBasketAsync(Arg.Is<CustomerBasket>(b => b.UserId == userId), Arg.Any<CancellationToken>());
        }
    }
}
