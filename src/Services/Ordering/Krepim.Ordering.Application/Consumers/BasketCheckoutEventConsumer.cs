using Krepim.EventBus.Events.Basket;
using Krepim.EventBus.Events.Inventory;
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
        IPublishEndpoint publishEndpoint,
        ILogger<BasketCheckoutEventConsumer> logger) : IConsumer<BasketCheckoutIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<BasketCheckoutIntegrationEvent> context)
        {
            var message = context.Message;
            logger.LogInformation("Получено событие оформления заказа для пользователя {UserId}", message.UserId);

            var address = new Address(message.FullAddress, message.Latitude, message.Longitude, message.Flat);
            var order = Order.Create(message.OrderId, message.UserId, address);

            foreach (var item in message.Items)
                order.AddOrderItem(item.ProductId, item.UnitPrice, item.Quantity);

            await orderRepository.AddAsync(order, context.CancellationToken);
            await unitOfWork.SaveChangesAsync(context.CancellationToken);

            var eventItems = message.Items.Select(i => new OrderItemPayload(
                i.ProductId,
                i.Quantity
            )).ToList();

            var orderCreatedEvent = new OrderCreatedIntegrationEvent(order.Id, order.TotalPrice, eventItems);

            await publishEndpoint.Publish(orderCreatedEvent, context.CancellationToken);

            logger.LogInformation("Успешно создан заказ {OrderId} для пользователя {UserId}", order.Id, message.UserId);
        }
    }
}
