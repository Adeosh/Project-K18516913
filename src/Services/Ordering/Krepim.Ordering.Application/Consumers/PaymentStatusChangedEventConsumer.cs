using Krepim.EventBus.Events.Payment;
using Krepim.Ordering.Domain.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Krepim.Ordering.Application.Consumers
{
    public sealed class PaymentStatusChangedEventConsumer(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<PaymentStatusChangedEventConsumer> logger) : IConsumer<PaymentStatusChangedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<PaymentStatusChangedIntegrationEvent> context)
        {
            var orderId = context.Message.OrderId;

            var order = await orderRepository.GetByIdAsync(orderId, context.CancellationToken);
            if (order == null)
                return;

            order.HandlePaymentResult(context.Message.Status);

            await unitOfWork.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation("Статус заказа {OrderId} изменен на {Status}", orderId, order.Status);
        }
    }
}
