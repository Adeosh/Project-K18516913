using Krepim.Basket.Domain.Entities;
using Krepim.Basket.Domain.Interfaces;
using Krepim.EventBus.Events.Basket;
using Krepim.SharedKernel.Results;
using MassTransit;
using MediatR;

namespace Krepim.Basket.Application.Features.Checkout
{
    internal sealed class CheckoutBasketCommandHandler(
        IBasketRepository repository,
        IPublishEndpoint publishEndpoint) : IRequestHandler<CheckoutBasketCommand, Result>
    {
        public async Task<Result> Handle(CheckoutBasketCommand request, CancellationToken ct)
        {
            CustomerBasket? basket = await repository.GetBasketAsync(request.UserId, ct);

            if (basket is null || !basket.Items.Any())
            {
                return Result.Failure(new Error(
                    "Basket.Empty",
                    "Невозможно оформить заказ: корзина пуста.",
                    ErrorType.Validation));
            }

            List<BasketCheckoutItem> eventItems = basket.Items
                .Select(i => new BasketCheckoutItem(i.ProductId, i.UnitPrice, i.Quantity))
                .ToList();

            BasketCheckoutIntegrationEvent checkoutEvent = new BasketCheckoutIntegrationEvent(
                request.OrderId,
                request.UserId,
                request.CustomerEmail,
                request.CustomerPhone,
                basket.TotalPrice,
                request.FullAddress,
                request.Latitude,
                request.Longitude,
                request.Flat,
                eventItems);

            await publishEndpoint.Publish(checkoutEvent, ct);
            await repository.DeleteBasketAsync(request.UserId, ct);

            return Result.Success();
        }
    }
}
