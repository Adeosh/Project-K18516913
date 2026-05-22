using Krepim.EventBus.Events.Basket;
using Krepim.Ordering.Application.Interfaces;
using Krepim.Ordering.Domain.Entities;
using Krepim.Ordering.Domain.ValueObjects;
using Krepim.SharedKernel.Domain.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Krepim.Ordering.Application.Consumers
{
    public sealed class BasketCheckoutEventConsumer(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<BasketCheckoutEventConsumer> logger) : IConsumer<BasketCheckoutIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<BasketCheckoutIntegrationEvent> context)
        {
            var message = context.Message;
            logger.LogInformation("Получено событие оформления заказа для пользователя {UserId}", message.UserId);

            var address = new Address(message.City, message.Street, message.ZipCode);
            var order = Order.Create(message.UserId, address);

            foreach (var item in message.Items)
                order.AddOrderItem(item.ProductId, item.UnitPrice, item.Quantity);

            await orderRepository.AddAsync(order, context.CancellationToken);
            await unitOfWork.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation("Успешно создан заказ {OrderId} для пользователя {UserId}", order.Id, message.UserId);
        }
    }
}
