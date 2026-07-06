using FluentAssertions;
using Krepim.Basket.Application.Features.UpdateItemQuantity;
using Krepim.Basket.Domain.Entities;
using Krepim.Basket.Domain.Interfaces;
using NSubstitute;

namespace Krepim.Basket.Application.Tests.Features
{
    public class UpdateItemQuantityCommandHandlerTests
    {
        private readonly IBasketRepository _repository;
        private readonly UpdateItemQuantityCommandHandler _handler;

        public UpdateItemQuantityCommandHandlerTests()
        {
            _repository = Substitute.For<IBasketRepository>();
            _handler = new UpdateItemQuantityCommandHandler(_repository);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenBasketNotFound()
        {
            // Arrange
            var command = new UpdateItemQuantityCommand(Guid.NewGuid(), Guid.NewGuid(), 1, 100m);
            _repository.GetBasketAsync(command.UserId, Arg.Any<CancellationToken>()).Returns((CustomerBasket?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Basket.NotFound");
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenItemNotFoundInBasket()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var basket = new CustomerBasket(userId);
            var command = new UpdateItemQuantityCommand(userId, Guid.NewGuid(), 1, 100m);

            _repository.GetBasketAsync(userId, Arg.Any<CancellationToken>()).Returns(basket);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Basket.ItemNotFound");
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenDomainValidationFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var basket = new CustomerBasket(userId);
            basket.AddItem(new BasketItem(productId, "Товар", "SKU", 100m, 1));

            var command = new UpdateItemQuantityCommand(userId, productId, 1, -10m);

            _repository.GetBasketAsync(userId, Arg.Any<CancellationToken>()).Returns(basket);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Basket.Invalid");
            result.Error.Description.Should().Contain("Price cannot be negative");
        }

        [Fact]
        public async Task Handle_Should_UpdateAndSave_WhenDataIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var basket = new CustomerBasket(userId);
            basket.AddItem(new BasketItem(productId, "Товар", "SKU", 100m, 1));

            var command = new UpdateItemQuantityCommand(userId, productId, 5, 200m);

            _repository.GetBasketAsync(userId, Arg.Any<CancellationToken>()).Returns(basket);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.First().Quantity.Should().Be(5);
            result.Value.Items.First().UnitPrice.Should().Be(200m);

            await _repository.Received(1).UpdateBasketAsync(basket, Arg.Any<CancellationToken>());
        }
    }
}
