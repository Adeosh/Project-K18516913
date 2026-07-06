using Krepim.Catalog.Application.Models.DTOs;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.Catalog.Domain.Enums;
using Krepim.SharedKernel.ValueObjects;

namespace Krepim.Catalog.Application.Tests
{
    public static class ProductTestFactory
    {
        private const string DefaultName = "Заклепка резьбовая";
        private const string DefaultDescription = "Заклепка резьбовая цилиндрическая";
        private const string DefaultSku = "ZRM-M6X15-ST-ZN";
        private const decimal DefaultPrice = 4.50m;
        private const string DefaultStandard = "DIN 7338";
        private const SalesUnit DefaultSalesUnit = SalesUnit.Pcs;
        private const decimal DefaultSalesStep = 250m;

        public static Product CreateActive(Guid? id = null)
        {
            var product = CreateBase(id);
            product.Publish();
            return product;
        }

        public static Product CreateInactive(Guid? id = null)
        {
            return CreateBase(id);
        }

        public static Product CreateDeleted(Guid? id = null)
        {
            var product = CreateBase(id);
            product.Delete();
            return product;
        }

        public static Product CreateWithCustomData(
            string? name = null,
            string? description = null,
            string? skuValue = null,
            decimal? price = null,
            Guid? categoryId = null,
            string? standard = null,
            SalesUnit? salesUnit = null,
            decimal? salesStep = null,
            Guid? id = null,
            bool isActive = false,
            bool isDeleted = false)
        {
            var result = Product.Create(
                name: name ?? DefaultName,
                description: description ?? DefaultDescription,
                skuValue: skuValue ?? DefaultSku,
                price: price ?? DefaultPrice,
                categoryId: categoryId ?? Guid.NewGuid(),
                standard: standard ?? DefaultStandard,
                salesUnit: salesUnit ?? DefaultSalesUnit,
                salesStep: salesStep ?? DefaultSalesStep);

            var product = result.Value;
            SetId(product, id ?? Guid.NewGuid());

            if (isActive)
                product.Publish();

            if (isDeleted)
                product.Delete();

            return product;
        }

        private static Product CreateBase(Guid? id)
        {
            var result = Product.Create(
                name: DefaultName,
                description: DefaultDescription,
                skuValue: DefaultSku,
                price: DefaultPrice,
                categoryId: Guid.NewGuid(),
                standard: DefaultStandard,
                salesUnit: DefaultSalesUnit,
                salesStep: DefaultSalesStep);

            var product = result.Value;
            SetId(product, id ?? Guid.NewGuid());
            return product;
        }

        private static void SetId(Product product, Guid id)
        {
            typeof(Product).GetProperty("Id")?.SetValue(product, id);
        }

        public static Product WithImages(this Product product, params string[] imageUrls)
        {
            product.SetImages(imageUrls);
            return product;
        }

        public static Product WithAttributes(this Product product, Dictionary<string, string> attributes)
        {
            product.SetAttributes(attributes);
            return product;
        }

        public static Product WithPriceTiers(this Product product, params PriceTier[] tiers)
        {
            product.UpdatePriceTiers(tiers);
            return product;
        }

        public static ProductReadDto CreateProductDto(Guid id, string name, string sku, bool isActive)
        {
            return new ProductReadDto(
                Id: id,
                Name: name,
                Description: $"Описание для {name}",
                Sku: sku,
                Price: 10.00m,
                Currency: "RUB",
                CategoryId: Guid.NewGuid(),
                IsActive: isActive,
                ImageUrls: new List<string>(),
                Standard: null,
                SalesUnit: (int)SalesUnit.Pcs,
                SalesStep: 1m,
                Attributes: new Dictionary<string, string>(),
                PriceTiers: new List<PriceTierDto>()
            );
        }
    }
}
