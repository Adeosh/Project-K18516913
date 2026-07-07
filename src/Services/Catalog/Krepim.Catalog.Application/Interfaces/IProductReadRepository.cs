using Krepim.Catalog.Application.Models.DTOs;

namespace Krepim.Catalog.Application.Interfaces
{
    public interface IProductReadRepository
    {
        Task<ProductReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IReadOnlyList<ProductReadDto>> SearchAsync(string searchTerm, bool onlyActive, int page, int pageSize, Guid? categoryId, CancellationToken cancellationToken);
    }
}
