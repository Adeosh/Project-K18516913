using Krepim.Identity.Domain.Aggregates;

namespace Krepim.Identity.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken);
        Task AddAsync(User user, CancellationToken cancellationToken);
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    }
}
