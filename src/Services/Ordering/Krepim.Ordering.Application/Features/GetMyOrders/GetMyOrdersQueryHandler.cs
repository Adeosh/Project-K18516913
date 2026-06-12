using Krepim.Ordering.Application.Models.DTOs;
using Krepim.Ordering.Domain.Interfaces;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Ordering.Application.Features.GetMyOrders
{
    internal sealed class GetMyOrdersQueryHandler(IOrderRepository repository)
        : IRequestHandler<GetMyOrdersQuery, Result<List<OrderDto>>>
    {
        public async Task<Result<List<OrderDto>>> Handle(GetMyOrdersQuery request, CancellationToken ct)
        {
            var orders = await repository.GetByUserIdAsync(request.UserId, ct);
            var dtos = orders.Select(o => new OrderDto(
                o.Id,
                o.Status.ToString(),
                o.TotalPrice,
                o.ShippingAddress.FullAddress,
                o.ShippingAddress.Latitude,
                o.ShippingAddress.Longitude,
                o.ShippingAddress.Flat,
                o.CreatedAt,
                o.Items.Select(i => new OrderItemDto(i.ProductId, i.UnitPrice, i.Quantity)).ToList()
            )).ToList();

            return dtos;
        }
    }
}
