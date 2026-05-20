using Krepim.Catalog.Application.Models;

namespace Krepim.Catalog.Application.Interfaces
{
    public interface IProductReadRepository
    {
        Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IReadOnlyList<ProductReadModel>> SearchAsync(string searchTerm, bool onlyActive, int page, int pageSize, CancellationToken cancellationToken);
    }
}
