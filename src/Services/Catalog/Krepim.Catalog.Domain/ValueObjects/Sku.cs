namespace Krepim.Catalog.Domain.ValueObjects
{
    public sealed record Sku
    {
        public string Value { get; }

        private Sku(string value) => Value = value;

        public static Sku? Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 5)
                return null;

            return new Sku(value.Trim().ToUpperInvariant());
        }
    }
}
