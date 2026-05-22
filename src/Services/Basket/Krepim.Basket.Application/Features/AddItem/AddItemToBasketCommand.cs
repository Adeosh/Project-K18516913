using Krepim.Basket.Domain.Entities;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Basket.Application.Features.AddItem
{
    public sealed record AddItemToBasketCommand(
        Guid UserId,
        Guid ProductId,
        string ProductName,
        string Sku,
        decimal UnitPrice,
        int Quantity) : IRequest<Result<CustomerBasket>>;
}
