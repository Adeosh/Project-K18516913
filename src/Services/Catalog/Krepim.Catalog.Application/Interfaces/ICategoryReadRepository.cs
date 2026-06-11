using Krepim.Catalog.Application.Models;

namespace Krepim.Catalog.Application.Interfaces
{
    public interface ICategoryReadRepository
    {
        Task<IReadOnlyList<CategoryModel>> GetAllActiveAsync(CancellationToken cancellationToken);
    }
}
