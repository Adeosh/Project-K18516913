using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Domain.Aggregates;

namespace Krepim.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class ProductWriteRepository(CatalogDbContext dbContext) : IProductWriteRepository
    {
        public async Task AddAsync(Product product, CancellationToken cancellationToken)
        {
            await dbContext.Products.AddAsync(product, cancellationToken);
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await dbContext.Products.FindAsync([id], cancellationToken);
        }
    }
}
