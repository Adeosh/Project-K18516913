using Krepim.EventBus.Events.Payment;
using Krepim.Payment.Domain.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Krepim.Payment.Application.Features.CompletePayment
{
    internal sealed class ProcessPaymentWebhookCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ILogger<ProcessPaymentWebhookCommandHandler> logger) : IRequestHandler<ProcessPaymentWebhookCommand, Result>
    {
        public async Task<Result> Handle(ProcessPaymentWebhookCommand request, CancellationToken ct)
        {
            var transaction = await paymentRepository.GetByExternalIdAsync(request.ExternalPaymentId, ct);

            if (transaction is null)
            {
                logger.LogError("Получен вебхук для неизвестного платежа: {ExternalId}", request.ExternalPaymentId);
                return Result.Failure(new Error("Payment.NotFound", "Транзакция не найдена", ErrorType.NotFound));
            }

            var result = transaction.HandleStatus(request.Status, request.ErrorMessage);
            if (result.IsFailure)
                return result;

            await unitOfWork.SaveChangesAsync(ct);

            var integrationEvent = new PaymentStatusChangedIntegrationEvent(transaction.OrderId, transaction.Status);

            await publishEndpoint.Publish(integrationEvent, ct);

            logger.LogInformation(
                "Платеж {ExternalId} обработан. Новый статус: {Status}, заказ {OrderId}",
                request.ExternalPaymentId, transaction.Status, transaction.OrderId);

            return Result.Success();
        }
    }
}
