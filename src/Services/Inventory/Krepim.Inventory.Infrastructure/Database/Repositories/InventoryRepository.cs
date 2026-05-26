using Krepim.Inventory.Domain.Entities;
using Krepim.Inventory.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Krepim.Inventory.Infrastructure.Database.Repositories
{
    internal sealed class InventoryRepository(InventoryDbContext dbContext) : IInventoryRepository
    {
        public async Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken ct = default)
        {
            return await dbContext.StockItems
                .FirstOrDefaultAsync(x => x.ProductId == productId, ct);
        }

        public async Task AddAsync(StockItem stockItem, CancellationToken ct = default)
        {
            await dbContext.StockItems.AddAsync(stockItem, ct);
        }
    }
}
