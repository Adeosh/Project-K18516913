namespace Krepim.SharedKernel.Enums
{
    public enum OrderStatus
    {
        /// <summary> Ожидает проверки/оплаты </summary>
        Pending = 1,
        /// <summary> В процессе валидации (например, проверка остатков в каталоге) </summary>
        AwaitingValidation = 2,
        /// <summary> Оплачен </summary>
        Paid = 3,
        /// <summary> Отправлен </summary>
        Shipped = 4,
        /// <summary> Отменен </summary>
        Cancelled = 5
    }
}
