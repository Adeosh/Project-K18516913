using Krepim.Ordering.Application.Models;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Ordering.Application.Features.GetMyOrders
{
    public sealed record GetMyOrdersQuery(Guid UserId) : IRequest<Result<List<OrderModel>>>;
}
