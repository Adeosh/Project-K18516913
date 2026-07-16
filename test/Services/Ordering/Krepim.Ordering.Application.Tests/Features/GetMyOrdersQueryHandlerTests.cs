using FluentAssertions;
using Krepim.Ordering.Application.Features.GetMyOrders;
using Krepim.Ordering.Domain.Entities;
using Krepim.SharedKernel.ValueObjects;
using Moq;

namespace Krepim.Ordering.Application.Tests.Features
{
    public class GetMyOrdersQueryHandlerTests : OrderingApplicationTestBase
    {
        private readonly GetMyOrdersQueryHandler _handler;

        public GetMyOrdersQueryHandlerTests()
        {
            _handler = new GetMyOrdersQueryHandler(OrderRepositoryMock.Object, Configuration);
        }

        [Fact]
        public async Task Handle_Should_ReturnMappedOrderModels_When_OrdersExistForUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var address = new Address("Санкт-Петербург, Невский пр-кт", 10003.00, 104345.00, "33");

            var order = Order.Create(orderId, userId, "test@example.com", "+7 (999) 123-45-67", address);
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
            mappedOrder.CustomerEmail.Should().Be(order.CustomerEmail);
            mappedOrder.CustomerPhone.Should().Be(order.CustomerPhone);
            mappedOrder.Status.Should().Be("Pending");
            mappedOrder.TotalPrice.Should().Be(600m);
            mappedOrder.FullAddress.Should().Be("Санкт-Петербург, Невский пр-кт");
            mappedOrder.Latitude.Should().Be(10003.00);
            mappedOrder.Longitude.Should().Be(104345.00);
            mappedOrder.Flat.Should().Be("33");
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
