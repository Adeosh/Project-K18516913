using Krepim.Identity.Application.Interfaces;
using Krepim.Identity.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace Krepim.Identity.Infrastructure.Database.Repositories
{
    internal sealed class UserRepository(IdentityDbContext dbContext) : IUserRepository
    {
        public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken)
        {
            return !await dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken)
        {
            await dbContext.Users.AddAsync(user, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }
    }
}
