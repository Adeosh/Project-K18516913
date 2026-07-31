using Krepim.EventBus.Events.Inventory;
using Krepim.Payment.Application.Interfaces;
using Krepim.Payment.Application.Models.Exchange;
using Krepim.Payment.Domain.Entities;
using Krepim.Payment.Domain.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
using MassTransit;
using Microsoft.Extensions.Logging;


namespace Krepim.Payment.Application.Consumers
{
    public sealed class OrderCreatedEventConsumer(
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork,
        ILogger<OrderCreatedEventConsumer> logger) : IConsumer<OrderCreatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
        {
            OrderCreatedIntegrationEvent message = context.Message;
            logger.LogInformation("Инициация платежа для заказа {OrderId} на сумму {Amount}", message.OrderId, message.TotalPrice);

            PaymentTransaction? existingTransaction = await paymentRepository.GetByOrderIdAsync(message.OrderId, context.CancellationToken);
            if (existingTransaction is not null)
            {
                logger.LogWarning("Платеж для заказа {OrderId} уже существует. Пропуск.", message.OrderId);
                return;
            }

            Result<PaymentTransaction> transactionResult = PaymentTransaction.Create(message.OrderId, message.TotalPrice);
            if (transactionResult.IsFailure)
                throw new InvalidOperationException($"Ошибка создания платежа: {transactionResult.Error.Description}");

            PaymentTransaction transaction = transactionResult.Value;

            PaymentGatewayResponse gatewayResponse = await paymentGateway.InitializePaymentAsync(message.OrderId, message.TotalPrice, context.CancellationToken);

            transaction.SetExternalDetails(gatewayResponse.ExternalId, gatewayResponse.PaymentUrl);

            await paymentRepository.AddAsync(transaction, context.CancellationToken);
            await unitOfWork.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation("Платеж {TransactionId} успешно инициализирован. Внешний ID: {ExternalId}. Ссылка на оплату: {Url}",
                transaction.Id, gatewayResponse.ExternalId, gatewayResponse.PaymentUrl);
        }
    }
}
