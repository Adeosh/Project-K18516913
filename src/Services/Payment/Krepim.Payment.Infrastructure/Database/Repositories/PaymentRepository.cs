using Krepim.Payment.Application.Interfaces;
using Krepim.Payment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Krepim.Payment.Infrastructure.Database.Repositories
{
    internal sealed class PaymentRepository(PaymentDbContext dbContext) : IPaymentRepository
    {
        public async Task<PaymentTransaction?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await dbContext.Transactions.FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<PaymentTransaction?> GetByExternalIdAsync(string externalId, CancellationToken ct = default)
        {
            return await dbContext.Transactions.FirstOrDefaultAsync(x => x.ExternalPaymentId == externalId, ct);
        }

        public async Task<PaymentTransaction?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await dbContext.Transactions.FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
        }

        public async Task AddAsync(PaymentTransaction transaction, CancellationToken ct = default)
        {
            await dbContext.Transactions.AddAsync(transaction, ct);
        }
    }
}
