using Krepim.Payment.Domain.Entities;
using Krepim.SharedKernel.Domain.Abstractions;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Krepim.Payment.Infrastructure.Database
{
    public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : DbContext(options), IUnitOfWork
    {
        public DbSet<PaymentTransaction> Transactions => Set<PaymentTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            base.OnModelCreating(modelBuilder);
        }
    }
}
