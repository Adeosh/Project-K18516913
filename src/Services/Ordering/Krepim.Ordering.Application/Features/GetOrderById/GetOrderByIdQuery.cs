using Krepim.Ordering.Application.Models.DTOs;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Ordering.Application.Features.GetOrderById
{
    public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<OrderDto>>;
}
