using Krepim.Basket.Domain.Entities;
using Krepim.Basket.Domain.Interfaces;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Basket.Application.Features.AddItem
{
    internal sealed class AddItemToBasketCommandHandler(IBasketRepository repository)
        : IRequestHandler<AddItemToBasketCommand, Result<CustomerBasket>>
    {
        public async Task<Result<CustomerBasket>> Handle(AddItemToBasketCommand request, CancellationToken ct)
        {
            CustomerBasket basket = await repository.GetBasketAsync(request.UserId, ct) ?? new CustomerBasket(request.UserId);

            try
            {
                BasketItem item = new BasketItem(
                    request.ProductId,
                    request.ProductName,
                    request.Sku,
                    request.UnitPrice,
                    request.Quantity);

                basket.AddItem(item);
            }
            catch (ArgumentException ex)
            {
                return Result<CustomerBasket>.Failure(
                    new Error("Basket.InvalidItem", ex.Message, ErrorType.Validation));
            }

            await repository.UpdateBasketAsync(basket, ct);

            return basket;
        }
    }
}
