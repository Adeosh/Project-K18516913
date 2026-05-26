using Krepim.Basket.Domain.Entities;
using Krepim.Basket.Domain.Interfaces;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Basket.Application.Features.GetBasket
{
    internal sealed class GetBasketQueryHandler(IBasketRepository repository)
        : IRequestHandler<GetBasketQuery, Result<CustomerBasket>>
    {
        public async Task<Result<CustomerBasket>> Handle(GetBasketQuery request, CancellationToken ct)
        {
            var basket = await repository.GetBasketAsync(request.UserId, ct);

            return basket ?? new CustomerBasket(request.UserId);
        }
    }
}
