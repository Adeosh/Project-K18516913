using FluentAssertions;
using Krepim.Basket.Application.Features.Checkout;
using Krepim.Basket.Domain.Entities;
using Krepim.Basket.Domain.Interfaces;
using Krepim.EventBus.Events.Basket;
using MassTransit;
using NSubstitute;

namespace Krepim.Basket.Application.Tests.Features
{
    public class CheckoutBasketCommandHandlerTests
    {
        private readonly IBasketRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly CheckoutBasketCommandHandler _handler;

        public CheckoutBasketCommandHandlerTests()
        {
            _repository = Substitute.For<IBasketRepository>();
            _publishEndpoint = Substitute.For<IPublishEndpoint>();
            _handler = new CheckoutBasketCommandHandler(_repository, _publishEndpoint);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenBasketIsEmpty()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new CheckoutBasketCommand(userId, Guid.NewGuid(), "test@example.com", "+7 (999) 123-45-67", "Addr", 0, 0, "1");
            _repository.GetBasketAsync(userId, Arg.Any<CancellationToken>()).Returns((CustomerBasket?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Basket.Empty");
        }

        [Fact]
        public async Task Handle_Should_PublishEventAndClearBasket_WhenBasketIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var basket = new CustomerBasket(userId);
            basket.AddItem(new BasketItem(Guid.NewGuid(), "Товар", "SKU1", 100m, 1));

            var command = new CheckoutBasketCommand(userId, orderId, "test@example.com", "+7 (999) 123-45-67", "ул. Пушкина", 55.75, 37.61, "1");

            _repository.GetBasketAsync(userId, Arg.Any<CancellationToken>()).Returns(basket);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            await _publishEndpoint.Received(1).Publish(
                Arg.Is<BasketCheckoutIntegrationEvent>(e =>
                    e.OrderId == orderId &&
                    e.UserId == userId &&
                    e.TotalPrice == basket.TotalPrice),
                Arg.Any<CancellationToken>());

            await _repository.Received(1).DeleteBasketAsync(userId, Arg.Any<CancellationToken>());
        }
    }
}
