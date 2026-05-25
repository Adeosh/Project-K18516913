using Krepim.Inventory.Domain.Entities;
using Krepim.SharedKernel.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Krepim.Inventory.Infrastructure.Database
{
    public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options)
    : DbContext(options), IUnitOfWork
    {
        public DbSet<StockItem> StockItems => Set<StockItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
