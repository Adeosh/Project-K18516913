namespace Krepim.Payment.Domain.Enums
{
    public enum PaymentStatus
    {
        /// <summary> Создан, ожидаем действий от пользователя/шлюза </summary>
        Pending = 1,
        /// <summary> Успешно оплачен </summary>
        Succeeded = 2,
        /// <summary> Ошибка оплаты (недостаточно средств, отказ банка) </summary>
        Failed = 3,
        /// <summary> Оформлен возврат </summary>
        Refunded = 4
    }
}
