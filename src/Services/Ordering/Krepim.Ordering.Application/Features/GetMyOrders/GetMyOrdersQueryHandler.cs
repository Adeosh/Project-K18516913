using Krepim.Ordering.Application.Models;
using Krepim.Ordering.Domain.Interfaces;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Ordering.Application.Features.GetMyOrders
{
    internal sealed class GetMyOrdersQueryHandler(IOrderRepository repository)
        : IRequestHandler<GetMyOrdersQuery, Result<List<OrderModel>>>
    {
        public async Task<Result<List<OrderModel>>> Handle(GetMyOrdersQuery request, CancellationToken ct)
        {
            var orders = await repository.GetByUserIdAsync(request.UserId, ct);
            var dtos = orders.Select(o => new OrderModel(
                o.Id,
                o.Status.ToString(),
                o.TotalPrice,
                o.ShippingAddress.City,
                o.ShippingAddress.Street,
                o.ShippingAddress.ZipCode,
                o.CreatedAt,
                o.Items.Select(i => new OrderItemModel(i.ProductId, i.UnitPrice, i.Quantity)).ToList()
            )).ToList();

            return dtos;
        }
    }
}
