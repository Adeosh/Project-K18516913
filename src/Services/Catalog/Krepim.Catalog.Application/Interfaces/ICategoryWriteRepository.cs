using Krepim.Catalog.Domain.Aggregates;

namespace Krepim.Catalog.Application.Interfaces
{
    public interface ICategoryWriteRepository
    {
        Task AddAsync(Category category, CancellationToken cancellationToken);
        Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
