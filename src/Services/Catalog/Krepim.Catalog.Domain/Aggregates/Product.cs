using Krepim.Catalog.Domain.ValueObjects;
using Krepim.SharedKernel.Domain;
using Krepim.SharedKernel.Results;

namespace Krepim.Catalog.Domain.Aggregates
{
    public sealed class Product : AggregateRoot<Guid>
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Sku Sku { get; private set; }
        public Money Price { get; private set; }
        public Guid CategoryId { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsDeleted { get; private set; }

        private Product(Guid id, string name, string description, Sku sku, Money price, Guid categoryId)
            : base(id)
        {
            Name = name;
            Description = description;
            Sku = sku;
            Price = price;
            CategoryId = categoryId;
            IsActive = false; // По умолчанию товар скрыт, пока менеджер не добавит фото/описание
        }

        #region For EF

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private Product() : base(Guid.Empty)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
        }
        #endregion

        public static Result<Product> Create(string name, string description, string skuValue, decimal price, Guid categoryId)
        {
            var sku = Sku.Create(skuValue);
            if (sku is null)
                return Result<Product>.Failure(new Error("Product.InvalidSku", "Invalid SKU format.", ErrorType.Validation));

            if (price <= 0)
                return Result<Product>.Failure(new Error("Product.InvalidPrice", "Price must be greater than zero.", ErrorType.Validation));

            var product = new Product(Guid.NewGuid(), name, description, sku, Money.Rubles(price), categoryId);

            return product;
        }

        public void UpdateDetails(string name, string description, Guid categoryId)
        {
            Name = name;
            Description = description;
            CategoryId = categoryId;
        }

        public void UpdatePrice(Money newPrice)
        {
            Price = newPrice;
        }

        public void Publish() => IsActive = true;

        public void Deactivate() => IsActive = false;

        public void Delete()
        {
            IsDeleted = true;
            IsActive = false;
        }
    }
}
