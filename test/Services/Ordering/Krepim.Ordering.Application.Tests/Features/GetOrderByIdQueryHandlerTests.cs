using FluentAssertions;
using Krepim.Ordering.Application.Features.GetOrderById;
using Krepim.Ordering.Domain.Entities;
using Krepim.SharedKernel.ValueObjects;
using Moq;

namespace Krepim.Ordering.Application.Tests.Features
{
    public class GetOrderByIdQueryHandlerTests : OrderingApplicationTestBase
    {
        private readonly GetOrderByIdQueryHandler _handler;

        public GetOrderByIdQueryHandlerTests()
        {
            _handler = new GetOrderByIdQueryHandler(OrderRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnMappedDto_When_OrderExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var address = new Address("Санкт-Петербург, ул.Тестовая", 10.0, 20.0, "5");
            var order = Order.Create(orderId, Guid.NewGuid(), address);
            order.AddOrderItem(Guid.NewGuid(), 100m, 2); // 200m total

            OrderRepositoryMock
                .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var query = new GetOrderByIdQuery(orderId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(orderId);
            result.Value.TotalPrice.Should().Be(200m);
            result.Value.FullAddress.Should().Be("Санкт-Петербург, ул.Тестовая");
            result.Value.QrCodeUrl.Should().NotBeNullOrEmpty();
            result.Value.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_OrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            OrderRepositoryMock
                .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);

            var query = new GetOrderByIdQuery(orderId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Order.NotFound");
        }
    }
}
