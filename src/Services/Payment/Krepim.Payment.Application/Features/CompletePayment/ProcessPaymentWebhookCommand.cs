using Krepim.SharedKernel.Enums;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Payment.Application.Features.CompletePayment
{
    public sealed record ProcessPaymentWebhookCommand(string ExternalPaymentId, PaymentStatus Status, string? ErrorMessage = null) : IRequest<Result>;
}
