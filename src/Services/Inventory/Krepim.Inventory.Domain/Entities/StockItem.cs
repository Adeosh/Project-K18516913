using Krepim.SharedKernel.Domain;
using Krepim.SharedKernel.Results;
using Error = Krepim.SharedKernel.Results.Error;

namespace Krepim.Inventory.Domain.Entities
{
    public sealed class StockItem : AggregateRoot<Guid>
    {
        public Guid ProductId { get; private set; }
        public int TotalQuantity { get; private set; }
        public int ReservedQuantity { get; private set; }
        public int AvailableQuantity => TotalQuantity - ReservedQuantity;

        #region For EF
        private StockItem() : base(Guid.NewGuid()) { }
        #endregion

        private StockItem(Guid id, Guid productId, int initialQuantity) : base(id)
        {
            ProductId = productId;
            TotalQuantity = initialQuantity;
            ReservedQuantity = 0;
        }

        public static Result<StockItem> Create(Guid productId, int initialQuantity)
        {
            if (initialQuantity < 0)
            {
                return Result<StockItem>.Failure(new Error(
                    "Inventory.InvalidQuantity",
                    "Начальное количество товара не может быть отрицательным.",
                    ErrorType.Validation));
            }

            return new StockItem(Guid.NewGuid(), productId, initialQuantity);
        }

        public Result CreditStock(int quantity)
        {
            if (quantity <= 0)
            {
                return Result.Failure(new Error(
                    "Inventory.InvalidOperation",
                    "Количество для поступления должно быть больше нуля.",
                    ErrorType.Validation));
            }

            TotalQuantity += quantity;
            return Result.Success();
        }

        public Result ReserveStock(int quantity)
        {
            if (quantity <= 0)
            {
                return Result.Failure(new Error(
                    "Inventory.InvalidOperation",
                    "Количество для резерва должно быть больше нуля.",
                    ErrorType.Validation));
            }

            if (quantity > AvailableQuantity)
            {
                return Result.Failure(new Error(
                    "Inventory.InsufficientStock",
                    $"Недостаточно свободного товара для резерва. Запрошено: {quantity}, Доступно: {AvailableQuantity}.",
                    ErrorType.Validation));
            }

            ReservedQuantity += quantity;
            return Result.Success();
        }

        public Result ConfirmReservation(int quantity)
        {
            if (quantity <= 0)
            {
                return Result.Failure(new Error(
                    "Inventory.InvalidOperation",
                    "Количество для подтверждения должно быть больше нуля.",
                    ErrorType.Validation));
            }

            if (quantity > ReservedQuantity)
            {
                return Result.Failure(new Error(
                    "Inventory.InvalidReservationConfirm",
                    $"Нельзя списать из резерва больше, чем там находится. Запрошено: {quantity}, В резерве: {ReservedQuantity}.",
                    ErrorType.Validation));
            }

            TotalQuantity -= quantity;
            ReservedQuantity -= quantity;
            return Result.Success();
        }

        public Result CancelReservation(int quantity)
        {
            if (quantity <= 0)
            {
                return Result.Failure(new Error(
                    "Inventory.InvalidOperation",
                    "Количество для отмены резерва должно быть больше нуля.",
                    ErrorType.Validation));
            }

            if (quantity > ReservedQuantity)
            {
                return Result.Failure(new Error(
                    "Inventory.InvalidReservationCancel",
                    $"Нельзя отменить резерв на количество, превышающее текущий резерв. Запрошено: {quantity}, В резерве: {ReservedQuantity}.",
                    ErrorType.Validation));
            }

            ReservedQuantity -= quantity;
            return Result.Success();
        }
    }
}
