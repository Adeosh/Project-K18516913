using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Basket.Application.Features.Checkout
{
    public sealed record CheckoutBasketCommand(
        Guid UserId,
        string City,
        string Street,
        string ZipCode) : IRequest<Result>;
}
