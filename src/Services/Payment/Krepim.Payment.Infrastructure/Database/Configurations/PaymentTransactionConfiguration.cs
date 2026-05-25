using Krepim.Payment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Krepim.Payment.Infrastructure.Database.Configurations
{
    internal sealed class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("PaymentTransactions");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.ExternalPaymentId).HasFilter("\"ExternalPaymentId\" IS NOT NULL");
            builder.Property(x => x.PaymentUrl).HasMaxLength(1000);
            builder.HasIndex(x => x.OrderId).IsUnique();

            builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            builder.Property(x => x.ExternalPaymentId).HasMaxLength(200);
            builder.Property(x => x.ErrorMessage).HasMaxLength(500);
        }
    }
}
