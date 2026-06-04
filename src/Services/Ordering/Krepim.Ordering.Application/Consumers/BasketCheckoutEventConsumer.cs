using Krepim.EventBus.Events.Basket;
using Krepim.Ordering.Domain.Entities;
using Krepim.Ordering.Domain.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.ValueObjects;
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

            var address = new Address(message.FullAddress, message.Latitude, message.Longitude, message.Flat);
            var order = Order.Create(message.UserId, address);

            foreach (var item in message.Items)
                order.AddOrderItem(item.ProductId, item.UnitPrice, item.Quantity);

            await orderRepository.AddAsync(order, context.CancellationToken);
            await unitOfWork.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation("Успешно создан заказ {OrderId} для пользователя {UserId}", order.Id, message.UserId);
        }
    }
}
