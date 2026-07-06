using Krepim.SharedKernel.Enums;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Payment.Application.Features.ProcessPayment
{
    public sealed record ProcessPaymentWebhookCommand(string ExternalPaymentId, PaymentStatus Status, string? ErrorMessage = null) : IRequest<Result>;
}
