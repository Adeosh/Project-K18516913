using Krepim.Ordering.Application.Models.DTOs;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Ordering.Application.Features.GetAllOrders
{
    public record GetAllOrdersQuery() : IRequest<Result<List<OrderDto>>>;
}
