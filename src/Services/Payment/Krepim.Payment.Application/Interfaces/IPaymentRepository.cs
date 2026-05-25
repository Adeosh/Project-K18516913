using Krepim.Payment.Domain.Entities;

namespace Krepim.Payment.Application.Interfaces
{
    public interface IPaymentRepository
    {
        Task<PaymentTransaction?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PaymentTransaction?> GetByExternalIdAsync(string externalId, CancellationToken ct = default);
        Task<PaymentTransaction?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
        Task AddAsync(PaymentTransaction transaction, CancellationToken ct = default);
    }
}
