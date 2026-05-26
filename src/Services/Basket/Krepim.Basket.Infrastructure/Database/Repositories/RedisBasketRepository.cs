using Krepim.Basket.Domain.Entities;
using Krepim.Basket.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Krepim.Basket.Infrastructure.Database.Repositories
{
    internal sealed class RedisBasketRepository(
        IDistributedCache cache,
        ILogger<RedisBasketRepository> logger) : IBasketRepository
    {
        private readonly DistributedCacheEntryOptions _options = new()
        {
            SlidingExpiration = TimeSpan.FromDays(7)
        };

        public async Task<CustomerBasket?> GetBasketAsync(Guid userId, CancellationToken ct = default)
        {
            var cachedBasket = await cache.GetStringAsync(userId.ToString(), ct);

            if (string.IsNullOrEmpty(cachedBasket))
                return null;

            try
            {
                return JsonSerializer.Deserialize<CustomerBasket>(cachedBasket);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Ошибка десериализации корзины для пользователя {UserId}", userId);
                return null;
            }
        }

        public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket, CancellationToken ct = default)
        {
            var jsonBasket = JsonSerializer.Serialize(basket);

            await cache.SetStringAsync(basket.UserId.ToString(), jsonBasket, _options, ct);

            return basket;
        }

        public async Task DeleteBasketAsync(Guid userId, CancellationToken ct = default)
        {
            await cache.RemoveAsync(userId.ToString(), ct);
        }
    }
}
