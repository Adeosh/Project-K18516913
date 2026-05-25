using Krepim.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Krepim.Inventory.Infrastructure.Database.Configurations
{
    internal sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
    {
        public void Configure(EntityTypeBuilder<StockItem> builder)
        {
            builder.ToTable("StockItems");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.ProductId).IsUnique();

            builder.Property(x => x.TotalQuantity).IsRequired();
            builder.Property(x => x.ReservedQuantity).IsRequired();

            builder.Ignore(x => x.AvailableQuantity);

            builder.Property<uint>("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsRowVersion();
        }
    }
}
