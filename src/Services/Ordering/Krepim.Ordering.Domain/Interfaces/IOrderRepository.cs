using Krepim.Ordering.Domain.Entities;

namespace Krepim.Ordering.Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<List<Order>> GetAllOrdersAsync(CancellationToken ct = default);
        Task AddAsync(Order order, CancellationToken ct = default);
    }
}
