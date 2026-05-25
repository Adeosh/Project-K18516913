using Krepim.Inventory.Domain.Entities;

namespace Krepim.Inventory.Application.Interfaces
{
    public interface IInventoryRepository
    {
        Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
        Task AddAsync(StockItem stockItem, CancellationToken ct = default);
    }
}
