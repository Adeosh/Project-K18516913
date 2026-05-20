using Krepim.Identity.Domain.Aggregates;
using Krepim.SharedKernel.Domain.Abstractions;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Krepim.Identity.Infrastructure.Database
{
    public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options), IUnitOfWork
    {
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            base.OnModelCreating(modelBuilder);
        }
    }
}
