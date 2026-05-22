using Krepim.Ordering.Application.Interfaces;
using Krepim.Ordering.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Krepim.Ordering.Infrastructure.Database.Repositories
{
    internal sealed class OrderRepository(OrderingDbContext dbContext) : IOrderRepository
    {
        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await dbContext.Orders
                .Include(x => x.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await dbContext.Orders
                .Include(x => x.Items)
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task AddAsync(Order order, CancellationToken ct = default)
        {
            await dbContext.Orders.AddAsync(order, ct);
        }
    }
}
