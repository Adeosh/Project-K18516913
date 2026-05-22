using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Basket.Application.Features.ClearBasket
{
    public sealed record ClearBasketCommand(Guid UserId) : IRequest<Result>;
}
