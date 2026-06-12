using Krepim.Basket.Domain.Entities;
using Krepim.Basket.Domain.Interfaces;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Basket.Application.Features.UpdateItemQuantity
{
    internal sealed class UpdateItemQuantityCommandHandler(IBasketRepository repository)
        : IRequestHandler<UpdateItemQuantityCommand, Result<CustomerBasket>>
    {
        public async Task<Result<CustomerBasket>> Handle(UpdateItemQuantityCommand request, CancellationToken ct)
        {
            var basket = await repository.GetBasketAsync(request.UserId, ct);
            if (basket is null) 
                return Result<CustomerBasket>.Failure(new Error("Basket.NotFound", "Корзина не найдена.", ErrorType.NotFound));

            var item = basket.Items.FirstOrDefault(x => x.ProductId == request.ProductId);
            if (item is null) 
                return Result<CustomerBasket>.Failure(new Error("Basket.ItemNotFound", "Товар не найден.", ErrorType.NotFound));

            try
            {
                item.UpdateQuantityAndPrice(request.Quantity, request.Price);
            }
            catch (ArgumentException ex)
            {
                return Result<CustomerBasket>.Failure(new Error("Basket.Invalid", ex.Message, ErrorType.Validation));
            }

            await repository.UpdateBasketAsync(basket, ct);
            return basket;
        }
    }
}
