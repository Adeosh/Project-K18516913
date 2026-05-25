using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Payment.Application.Features.CompletePayment
{
    public sealed record CompletePaymentCommand(string ExternalPaymentId) : IRequest<Result>;
}
