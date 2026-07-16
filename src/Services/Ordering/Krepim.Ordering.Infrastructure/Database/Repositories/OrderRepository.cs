using Krepim.Ordering.Domain.Entities;
using Krepim.Ordering.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Krepim.Ordering.Infrastructure.Database.Repositories
{
    internal sealed class OrderRepository(OrderingDbContext dbContext) : IOrderRepository
    {
        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await dbContext.Orders
                .Include(x => x.Items)
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

        public async Task<List<Order>> GetAllOrdersAsync(CancellationToken ct = default)
        {
            return await dbContext.Orders
                .Include(x => x.Items)
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task AddAsync(Order order, CancellationToken ct = default)
        {
            await dbContext.Orders.AddAsync(order, ct);
        }
    }
}
