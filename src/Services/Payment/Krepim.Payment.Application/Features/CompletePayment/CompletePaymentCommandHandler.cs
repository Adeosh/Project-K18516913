using Krepim.EventBus.Events.Payment;
using Krepim.Payment.Application.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Krepim.Payment.Application.Features.CompletePayment
{
    internal sealed class CompletePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ILogger<CompletePaymentCommandHandler> logger) : IRequestHandler<CompletePaymentCommand, Result>
    {
        public async Task<Result> Handle(CompletePaymentCommand request, CancellationToken ct)
        {
            var transaction = await paymentRepository.GetByExternalIdAsync(request.ExternalPaymentId, ct);

            if (transaction is null)
            {
                logger.LogError("Получен вебхук для неизвестного платежа: {ExternalId}", request.ExternalPaymentId);
                return Result.Failure(new Error("Payment.NotFound", "Транзакция не найдена", ErrorType.NotFound));
            }

            var result = transaction.MarkAsSucceeded();
            if (result.IsFailure)
                return result;

            var integrationEvent = new PaymentSucceededIntegrationEvent(transaction.OrderId);

            await publishEndpoint.Publish(integrationEvent, ct);
            await unitOfWork.SaveChangesAsync(ct);

            logger.LogInformation("Платеж {ExternalId} успешно завершен. Заказ {OrderId} может быть отгружен.",
                request.ExternalPaymentId, transaction.OrderId);

            return Result.Success();
        }
    }
}
