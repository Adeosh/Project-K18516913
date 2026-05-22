using Krepim.Basket.Domain.Entities;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Basket.Application.Features.GetBasket
{
    public sealed record GetBasketQuery(Guid UserId) : IRequest<Result<CustomerBasket>>;
}
