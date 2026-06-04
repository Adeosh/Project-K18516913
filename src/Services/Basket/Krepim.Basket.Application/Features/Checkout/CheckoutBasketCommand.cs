using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Basket.Application.Features.Checkout
{
    public sealed record CheckoutBasketCommand(
        Guid UserId,
        string FullAddress,
        double Latitude,
        double Longitude,
        string Flat) : IRequest<Result>;
}
