using Krepim.Catalog.Domain.Aggregates;
using Krepim.Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Krepim.Catalog.Infrastructure.Database.Configurations
{
    internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(2000);

            builder.Property(p => p.Sku)
                .HasConversion(
                    sku => sku.Value,
                    value => Sku.Create(value)!)
                .HasColumnName("Sku")
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(p => p.Sku).IsUnique();

            builder.ComplexProperty(p => p.Price, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount)
                    .HasColumnName("PriceAmount")
                    .HasColumnType("numeric(18,2)")
                    .IsRequired();

                priceBuilder.Property(m => m.Currency)
                    .HasColumnName("PriceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            builder.Property(p => p.IsActive)
                .HasDefaultValue(false);

            builder.Ignore(p => p.DomainEvents);
        }
    }
}
