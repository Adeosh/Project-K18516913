using FluentAssertions;
using Krepim.Basket.Application.Features.AddItem;
using Krepim.Basket.Application.Interfaces;
using Krepim.Basket.Domain.Entities;
using Krepim.SharedKernel.Results;
using Moq;

namespace Krepim.Basket.Application.Tests.Features
{
    public class AddItemToBasketCommandHandlerTests
    {
        private readonly Mock<IBasketRepository> _repositoryMock = new();

        [Fact]
        public async Task Handle_Should_AddProductAndSave_WhenDataIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var basket = new CustomerBasket(userId);

            _repositoryMock.Setup(repo => repo.GetBasketAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(basket);

            var handler = new AddItemToBasketCommandHandler(_repositoryMock.Object);
            var command = new AddItemToBasketCommand(userId, productId, "Test", "T-1", 500m, 2);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().ContainSingle(i => i.ProductId == productId && i.Quantity == 2);

            _repositoryMock.Verify(repo => repo.UpdateBasketAsync(It.IsAny<CustomerBasket>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnValidationFailure_WhenDomainThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var basket = new CustomerBasket(userId);

            _repositoryMock.Setup(repo => repo.GetBasketAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(basket);

            var handler = new AddItemToBasketCommandHandler(_repositoryMock.Object);
            var command = new AddItemToBasketCommand(userId, Guid.NewGuid(), "Test", "T-1", 500m, -1);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.Validation);

            _repositoryMock.Verify(repo => repo.UpdateBasketAsync(It.IsAny<CustomerBasket>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
