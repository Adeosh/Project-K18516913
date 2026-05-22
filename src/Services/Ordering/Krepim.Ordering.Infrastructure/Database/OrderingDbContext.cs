using Krepim.Ordering.Domain.Entities;
using Krepim.SharedKernel.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Krepim.Ordering.Infrastructure.Database
{
    public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options)
        : DbContext(options), IUnitOfWork
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
