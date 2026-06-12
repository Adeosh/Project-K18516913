using Krepim.Basket.Domain.Entities;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Basket.Application.Features.UpdateItemQuantity
{
    public sealed record UpdateItemQuantityCommand(Guid UserId, Guid ProductId, int Quantity, decimal Price) : IRequest<Result<CustomerBasket>>;
}
