using Krepim.Basket.Application.Interfaces;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Basket.Application.Features.ClearBasket
{
    internal sealed class ClearBasketCommandHandler(IBasketRepository repository)
        : IRequestHandler<ClearBasketCommand, Result>
    {
        public async Task<Result> Handle(ClearBasketCommand request, CancellationToken ct)
        {
            await repository.DeleteBasketAsync(request.UserId, ct);

            return Result.Success();
        }
    }
}
