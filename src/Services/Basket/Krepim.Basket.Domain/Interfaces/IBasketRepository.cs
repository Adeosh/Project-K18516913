using Krepim.Basket.Domain.Entities;

namespace Krepim.Basket.Domain.Interfaces
{
    public interface IBasketRepository
    {
        Task<CustomerBasket?> GetBasketAsync(Guid userId, CancellationToken ct = default);
        Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket, CancellationToken ct = default);
        Task DeleteBasketAsync(Guid userId, CancellationToken ct = default);
    }
}
