using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Krepim.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class CategoryReadRepository(CatalogDbContext dbContext) : ICategoryReadRepository
    {
        public async Task<IReadOnlyList<CategoryModel>> GetAllActiveAsync(CancellationToken cancellationToken)
        {
            return await dbContext.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .Select(c => new CategoryModel(c.Id, c.Name, c.Description))
                .ToListAsync(cancellationToken);
        }
    }
}
