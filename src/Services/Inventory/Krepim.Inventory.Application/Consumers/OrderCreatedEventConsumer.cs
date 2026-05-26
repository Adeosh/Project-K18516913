using Krepim.EventBus.Events.Inventory;
using Krepim.Inventory.Domain.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Krepim.Inventory.Application.Consumers
{
    public sealed class OrderCreatedEventConsumer(
        IInventoryRepository inventoryRepository,
        IUnitOfWork unitOfWork,
        ILogger<OrderCreatedEventConsumer> logger) : IConsumer<OrderCreatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
        {
            var message = context.Message;
            logger.LogInformation("Попытка резервирования товаров для заказа {OrderId}", message.OrderId);

            foreach (var item in message.Items)
            {
                var stockItem = await inventoryRepository.GetByProductIdAsync(item.ProductId, context.CancellationToken);

                if (stockItem is null)
                    throw new InvalidOperationException($"Товар {item.ProductId} не найден на складе.");

                var reserveResult = stockItem.ReserveStock(item.Quantity);

                if (reserveResult.IsFailure)
                    throw new InvalidOperationException(
                        $"Не удалось зарезервировать {item.Quantity} шт. товара {item.ProductId}. Ошибка: {reserveResult.Error.Description}");
            }

            await unitOfWork.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation("Успешно зарезервированы товары для заказа {OrderId}", message.OrderId);
        }
    }
}
