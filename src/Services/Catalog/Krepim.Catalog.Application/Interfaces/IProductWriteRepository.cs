using Krepim.Catalog.Domain.Aggregates;

namespace Krepim.Catalog.Application.Interfaces
{
    public interface IProductWriteRepository
    {
        Task AddAsync(Product product, CancellationToken cancellationToken);
        Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
