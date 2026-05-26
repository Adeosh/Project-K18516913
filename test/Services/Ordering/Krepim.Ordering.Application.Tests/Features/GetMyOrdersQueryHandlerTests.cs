using FluentAssertions;
using Krepim.Ordering.Application.Features.GetMyOrders;
using Krepim.Ordering.Domain.Entities;
using Krepim.Ordering.Domain.ValueObjects;
using Moq;

namespace Krepim.Ordering.Application.Tests.Features
{
    public class GetMyOrdersQueryHandlerTests : OrderingApplicationTestBase
    {
        private readonly GetMyOrdersQueryHandler _handler;

        public GetMyOrdersQueryHandlerTests()
        {
            _handler = new GetMyOrdersQueryHandler(OrderRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnMappedOrderModels_When_OrdersExistForUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var address = new Address("Санкт-Петербург", "Невский", "190000");

            var order = Order.Create(userId, address);
            order.AddOrderItem(Guid.NewGuid(), 200m, 3);

            var dbOrders = new List<Order> { order };

            OrderRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbOrders);

            var query = new GetMyOrdersQuery(userId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().HaveCount(1);

            var mappedOrder = result.Value.First();
            mappedOrder.Id.Should().Be(order.Id);
            mappedOrder.Status.Should().Be("Pending");
            mappedOrder.TotalPrice.Should().Be(600m);
            mappedOrder.City.Should().Be("Санкт-Петербург");
            mappedOrder.Street.Should().Be("Невский");
            mappedOrder.ZipCode.Should().Be("190000");
            mappedOrder.Items.Should().HaveCount(1);
            mappedOrder.Items.First().UnitPrice.Should().Be(200m);
            mappedOrder.Items.First().Quantity.Should().Be(3);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_When_NoOrdersFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            OrderRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Order>());

            var query = new GetMyOrdersQuery(userId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }
    }
}
