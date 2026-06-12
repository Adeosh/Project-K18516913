using Krepim.Catalog.Domain.Enums;
using Krepim.SharedKernel.Domain;
using Krepim.SharedKernel.Results;
using Krepim.SharedKernel.ValueObjects;

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
        public string? Standard { get; private set; } // "DIN 933", "ГОСТ 7798-70"
        public SalesUnit SalesUnit { get; private set; } // Шт, Кг, Упак
        public decimal SalesStep { get; private set; } // Шаг (например, 0.5 для кг или 100 для заклепок в пачке)

        private readonly Dictionary<string, string> _attributes = new();
        public IReadOnlyDictionary<string, string> Attributes => _attributes;

        private readonly List<PriceTier> _priceTiers = new();
        public IReadOnlyList<PriceTier> PriceTiers => _priceTiers.AsReadOnly();

        private readonly List<string> _imageUrls = new();
        public IReadOnlyList<string> ImageUrls => _imageUrls.AsReadOnly();

        private Product(
            Guid id,
            string name, 
            string description, 
            Sku sku, 
            Money price, 
            Guid categoryId,
            string? standard,
            SalesUnit salesUnit,
            decimal salesStep) : base(id)
        {
            Name = name;
            Description = description;
            Sku = sku;
            Price = price;
            CategoryId = categoryId;
            Standard = standard;
            SalesUnit = salesUnit;
            SalesStep = salesStep <= 0 ? 1 : salesStep;
            IsActive = false; // По умолчанию товар скрыт, пока менеджер не добавит фото/описание
        }

        #region For EF

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private Product() : base(Guid.Empty)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
        }
        #endregion

        public static Result<Product> Create(
            string name,
            string description,
            string skuValue,
            decimal price,
            Guid categoryId,
            string? standard,
            SalesUnit salesUnit,
            decimal salesStep)
        {
            var sku = Sku.Create(skuValue);

            if (sku is null)
                return Result<Product>.Failure(new Error("Product.InvalidSku", "Неверный формат артикула.", ErrorType.Validation));

            if (price <= 0)
                return Result<Product>.Failure(new Error("Product.InvalidPrice", "Цена должна быть больше нуля.", ErrorType.Validation));

            return new Product(Guid.NewGuid(), name, description, sku, Money.Rubles(price), categoryId, standard, salesUnit, salesStep);
        }

        public void UpdateDetails(
            string name,
            string description,
            Guid categoryId,
            string? standard,
            SalesUnit salesUnit,
            decimal salesStep)
        {
            Name = name;
            Description = description;
            CategoryId = categoryId;
            Standard = standard;
            SalesUnit = salesUnit;
            SalesStep = salesStep <= 0 ? 1 : salesStep;
        }

        public void UpdatePrice(Money newPrice)
        {
            Price = newPrice;
        }

        public void SetAttributes(Dictionary<string, string> attributes)
        {
            _attributes.Clear();
            foreach (var attr in attributes)
            {
                if (!string.IsNullOrWhiteSpace(attr.Key))
                    _attributes[attr.Key] = attr.Value;
            }
        }

        public void UpdatePriceTiers(IEnumerable<PriceTier> tiers)
        {
            _priceTiers.Clear();
            _priceTiers.AddRange(tiers.OrderBy(t => t.MinQuantity));
        }

        public void SetImages(IEnumerable<string> imageUrls)
        {
            _imageUrls.Clear();
            _imageUrls.AddRange(imageUrls);
        }

        public void AddImage(string imageUrl)
        {
            if (!_imageUrls.Contains(imageUrl))
                _imageUrls.Add(imageUrl);
        }

        public void RemoveImage(string imageUrl)
        {
            _imageUrls.Remove(imageUrl);
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
