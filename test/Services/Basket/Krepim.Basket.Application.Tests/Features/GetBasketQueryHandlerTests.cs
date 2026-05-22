using FluentAssertions;
using Krepim.Basket.Application.Features.GetBasket;
using Krepim.Basket.Application.Interfaces;
using Krepim.Basket.Domain.Entities;
using Moq;

namespace Krepim.Basket.Application.Tests.Features
{
    public class GetBasketQueryHandlerTests
    {
        private readonly Mock<IBasketRepository> _repositoryMock = new();

        [Fact]
        public async Task Handle_Should_ReturnExistingBasket_WhenFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingBasket = new CustomerBasket(userId);
            existingBasket.AddItem(new BasketItem(Guid.NewGuid(), "Test", "T-1", 100, 1));

            _repositoryMock.Setup(repo => repo.GetBasketAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingBasket);

            var handler = new GetBasketQueryHandler(_repositoryMock.Object);
            var query = new GetBasketQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(existingBasket);
        }

        [Fact]
        public async Task Handle_Should_ReturnNewEmptyBasket_WhenNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _repositoryMock.Setup(repo => repo.GetBasketAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CustomerBasket?)null);

            var handler = new GetBasketQueryHandler(_repositoryMock.Object);
            var query = new GetBasketQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.UserId.Should().Be(userId);
            result.Value.Items.Should().BeEmpty();
        }
    }
}
