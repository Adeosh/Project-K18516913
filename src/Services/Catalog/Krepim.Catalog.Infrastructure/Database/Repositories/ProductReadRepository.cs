using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models;
using MongoDB.Driver;

namespace Krepim.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class ProductReadRepository(IMongoDatabase mongoDatabase) : IProductReadRepository
    {
        private readonly IMongoCollection<ProductReadModel> _collection =
            mongoDatabase.GetCollection<ProductReadModel>("ProductsView");

        public async Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var filter = Builders<ProductReadModel>.Filter.Eq(x => x.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ProductReadModel>> SearchAsync(string searchTerm, bool onlyActive, int page, int pageSize, CancellationToken cancellationToken)
        {
            var filterBuilder = Builders<ProductReadModel>.Filter;
            var filter = filterBuilder.Empty;

            if (onlyActive)
            {
                filter &= filterBuilder.Eq(x => x.IsActive, true);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchRegex = new MongoDB.Bson.BsonRegularExpression(searchTerm, "i");
                filter &= filterBuilder.Or(
                    filterBuilder.Regex(x => x.Name, searchRegex),
                    filterBuilder.Regex(x => x.Sku, searchRegex)
                );
            }

            return await _collection.Find(filter).Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync(cancellationToken);
        }
    }
}
