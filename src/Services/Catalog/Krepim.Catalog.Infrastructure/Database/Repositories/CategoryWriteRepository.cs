using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Domain.Aggregates;

namespace Krepim.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class CategoryWriteRepository(CatalogDbContext dbContext) : ICategoryWriteRepository
    {
        public async Task AddAsync(Category category, CancellationToken cancellationToken)
        {
            await dbContext.Categories.AddAsync(category, cancellationToken);
        }

        public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await dbContext.Categories.FindAsync([id], cancellationToken);
        }
    }
}
