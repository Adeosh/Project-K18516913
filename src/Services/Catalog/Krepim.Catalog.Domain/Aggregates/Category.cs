using Krepim.SharedKernel.Domain;
using Krepim.SharedKernel.Results;

namespace Krepim.Catalog.Domain.Aggregates
{
    public sealed class Category : AggregateRoot<Guid>
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsDeleted { get; private set; }

        private Category(Guid id, string name, string description) : base(id)
        {
            Name = name;
            Description = description;
            IsActive = true;
            IsDeleted = false;
        }

        #region For EF
#pragma warning disable CS8618
        private Category() : base(Guid.Empty) { }
#pragma warning restore CS8618
        #endregion

        public static Result<Category> Create(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result<Category>.Failure(new Error("Category.EmptyName", "Название категории не может быть пустым.", ErrorType.Validation));

            return new Category(Guid.NewGuid(), name, description);
        }

        public void Update(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public void Deactivate() => IsActive = false;

        public void Activate() => IsActive = true;

        public void Delete()
        {
            IsDeleted = true;
            IsActive = false;
        }
    }
}
