using Krepim.Basket.Domain.Entities;
using Krepim.Basket.Domain.Interfaces;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Basket.Application.Features.RemoveItem
{
    internal sealed class RemoveItemFromBasketCommandHandler(IBasketRepository repository)
        : IRequestHandler<RemoveItemFromBasketCommand, Result<CustomerBasket>>
    {
        public async Task<Result<CustomerBasket>> Handle(RemoveItemFromBasketCommand request, CancellationToken ct)
        {
            var basket = await repository.GetBasketAsync(request.UserId, ct);

            if (basket is null)
                return new CustomerBasket(request.UserId);

            basket.RemoveItem(request.ProductId);

            await repository.UpdateBasketAsync(basket, ct);

            return basket;
        }
    }
}
