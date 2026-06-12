using Krepim.Catalog.Domain.Aggregates;
using Krepim.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

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

            builder.HasIndex(p => p.Sku)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false"); ;

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

            builder.Property(p => p.ImageUrls)
               .HasColumnName("ImageUrls")
               .HasColumnType("text[]")
               .IsRequired(false);

            ValueComparer<IReadOnlyDictionary<string, string>> attributesComparer = new ValueComparer<IReadOnlyDictionary<string, string>>(
                (c1, c2) => JsonSerializer.Serialize(c1, JsonSerializerOptions.Default) == JsonSerializer.Serialize(c2, JsonSerializerOptions.Default),
                c => c == null ? 0 : JsonSerializer.Serialize(c, JsonSerializerOptions.Default).GetHashCode(),
                c => JsonSerializer.Deserialize<Dictionary<string, string>>(JsonSerializer.Serialize(c, JsonSerializerOptions.Default), JsonSerializerOptions.Default) ?? new Dictionary<string, string>()
            );

            builder.Property(x => x.Attributes)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, JsonSerializerOptions.Default) ?? new Dictionary<string, string>())
                .Metadata.SetValueComparer(attributesComparer);

            ValueComparer<IReadOnlyList<PriceTier>> priceTiersComparer = new ValueComparer<IReadOnlyList<PriceTier>>(
                (c1, c2) => JsonSerializer.Serialize(c1, JsonSerializerOptions.Default) == JsonSerializer.Serialize(c2, JsonSerializerOptions.Default),
                c => c == null ? 0 : JsonSerializer.Serialize(c, JsonSerializerOptions.Default).GetHashCode(),
                c => JsonSerializer.Deserialize<List<PriceTier>>(JsonSerializer.Serialize(c, JsonSerializerOptions.Default), JsonSerializerOptions.Default) ?? new List<PriceTier>()
            );

            builder.Property(x => x.PriceTiers)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<List<PriceTier>>(v, JsonSerializerOptions.Default) ?? new List<PriceTier>())
                .Metadata.SetValueComparer(priceTiersComparer);

            builder.Ignore(p => p.DomainEvents);
        }
    }
}
