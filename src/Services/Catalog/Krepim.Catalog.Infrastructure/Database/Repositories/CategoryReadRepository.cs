using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Krepim.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class CategoryReadRepository(CatalogDbContext dbContext) : ICategoryReadRepository
    {
        public async Task<IReadOnlyList<CategoryDto>> GetAllActiveAsync(CancellationToken cancellationToken)
        {
            return await dbContext.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
                .ToListAsync(cancellationToken);
        }
    }
}
