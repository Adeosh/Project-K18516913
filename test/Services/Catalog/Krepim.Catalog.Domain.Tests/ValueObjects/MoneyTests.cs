using FluentAssertions;
using Krepim.Catalog.Domain.ValueObjects;

namespace Krepim.Catalog.Domain.Tests.ValueObjects
{
    public class MoneyTests
    {
        [Fact]
        public void OperatorPlus_Should_AddAmounts_WhenCurrenciesMatch()
        {
            // Arrange
            var m1 = Money.Rubles(100.50m);
            var m2 = Money.Rubles(50.25m);

            // Act
            var result = m1 + m2;

            // Assert
            result.Amount.Should().Be(150.75m);
            result.Currency.Should().Be("RUB");
        }

        [Fact]
        public void OperatorPlus_Should_ThrowException_WhenCurrenciesDiffer()
        {
            // Arrange
            var m1 = Money.Rubles(100);
            var m2 = new Money(50, "USD");

            // Act
            var action = () => { var res = m1 + m2; };

            // Assert
            action.Should().Throw<InvalidOperationException>()
                  .WithMessage("Cannot add different currencies");
        }
    }
}
