using Krepim.Catalog.Application.Models.DTOs;

namespace Krepim.Catalog.Application.Interfaces
{
    public interface ICategoryReadRepository
    {
        Task<IReadOnlyList<CategoryDto>> GetAllActiveAsync(CancellationToken cancellationToken);
    }
}
