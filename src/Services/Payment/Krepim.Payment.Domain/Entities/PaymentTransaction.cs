using Krepim.Payment.Domain.Enums;
using Krepim.SharedKernel.Domain;
using Krepim.SharedKernel.Results;
using Error = Krepim.SharedKernel.Results.Error;

namespace Krepim.Payment.Domain.Entities
{
    public sealed class PaymentTransaction : AggregateRoot<Guid>
    {
        public Guid OrderId { get; private set; }
        public decimal Amount { get; private set; }
        public PaymentStatus Status { get; private set; }
        public string? ExternalPaymentId { get; private set; } // Идентификатор транзакции
        public string? PaymentUrl { get; private set; }
        public string? ErrorMessage { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }

        #region For EF
        private PaymentTransaction() : base(Guid.NewGuid()) { }
        #endregion

        private PaymentTransaction(Guid id, Guid orderId, decimal amount) : base(id)
        {
            OrderId = orderId;
            Amount = amount;
            Status = PaymentStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<PaymentTransaction> Create(Guid orderId, decimal amount)
        {
            if (amount <= 0)
            {
                return Result<PaymentTransaction>.Failure(new Error(
                    "Payment.InvalidAmount",
                    "Сумма платежа должна быть больше нуля.",
                    ErrorType.Validation));
            }

            return new PaymentTransaction(Guid.NewGuid(), orderId, amount);
        }

        public void SetExternalId(string externalId)
        {
            if (string.IsNullOrWhiteSpace(externalId))
                throw new ArgumentException("Внешний ID не может быть пустым.", nameof(externalId));

            ExternalPaymentId = externalId;
        }

        public void SetExternalDetails(string externalId, string paymentUrl)
        {
            if (string.IsNullOrWhiteSpace(externalId))
                throw new ArgumentException("Внешний ID не может быть пустым.", nameof(externalId));
            if (string.IsNullOrWhiteSpace(paymentUrl))
                throw new ArgumentException("Ссылка на оплату не может быть пустой.", nameof(paymentUrl));

            ExternalPaymentId = externalId;
            PaymentUrl = paymentUrl;
        }

        public Result MarkAsSucceeded()
        {
            if (Status == PaymentStatus.Succeeded)
                return Result.Success();

            if (Status != PaymentStatus.Pending)
            {
                return Result.Failure(new Error(
                    "Payment.InvalidState",
                    $"Невозможно отметить платеж как успешный из статуса {Status}.",
                    ErrorType.Conflict));
            }

            Status = PaymentStatus.Succeeded;
            ProcessedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result MarkAsFailed(string error)
        {
            if (Status != PaymentStatus.Pending)
            {
                return Result.Failure(new Error(
                    "Payment.InvalidState",
                    $"Невозможно отметить платеж как ошибочный из статуса {Status}.",
                    ErrorType.Conflict));
            }

            Status = PaymentStatus.Failed;
            ErrorMessage = error;
            ProcessedAt = DateTime.UtcNow;
            return Result.Success();
        }
    }
}
