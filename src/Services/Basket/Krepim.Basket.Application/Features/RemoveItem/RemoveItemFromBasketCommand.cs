using Krepim.Basket.Domain.Entities;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Basket.Application.Features.RemoveItem
{
    public sealed record RemoveItemFromBasketCommand(Guid UserId, Guid ProductId) : IRequest<Result<CustomerBasket>>;
}
