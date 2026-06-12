using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models.DTOs;
using MongoDB.Driver;

namespace Krepim.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class ProductReadRepository(IMongoDatabase mongoDatabase) : IProductReadRepository
    {
        private readonly IMongoCollection<ProductReadDto> _collection =
            mongoDatabase.GetCollection<ProductReadDto>("ProductsView");

        public async Task<ProductReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var filter = Builders<ProductReadDto>.Filter.Eq(x => x.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ProductReadDto>> SearchAsync(string searchTerm, bool onlyActive, int page, int pageSize, CancellationToken cancellationToken)
        {
            var filterBuilder = Builders<ProductReadDto>.Filter;
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
