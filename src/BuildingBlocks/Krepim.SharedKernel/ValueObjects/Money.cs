namespace Krepim.SharedKernel.ValueObjects
{
    public readonly record struct Money(decimal Amount, string Currency)
    {
        public static Money Rubles(decimal amount) => new(amount, "RUB");

        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Cannot add different currencies");

            return new Money(a.Amount + b.Amount, a.Currency);
        }
    }
}
