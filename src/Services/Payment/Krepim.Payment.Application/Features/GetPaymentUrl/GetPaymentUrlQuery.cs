using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Payment.Application.Features.GetPaymentUrl
{
    public sealed record GetPaymentUrlQuery(Guid OrderId) : IRequest<Result<string>>;
}
