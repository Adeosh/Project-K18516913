using FluentAssertions;
using Krepim.SharedKernel.ValueObjects;


namespace Krepim.Catalog.Domain.Tests.ValueObjects
{
    public class SkuTests
    {
        [Theory]
        [InlineData("sku-123", "SKU-123")]
        [InlineData("  abcde  ", "ABCDE")]
        [InlineData("VALID-SKU", "VALID-SKU")]
        public void Create_Should_ReturnValidSku_AndFormatCorrectly(string input, string expected)
        {
            // Act
            var sku = Sku.Create(input);

            // Assert
            sku.Should().NotBeNull();
            sku!.Value.Should().Be(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("abc")]
        [InlineData(null)]
        public void Create_Should_ReturnNull_WhenInputIsInvalid(string? input)
        {
            // Act
            var sku = Sku.Create(input!);

            // Assert
            sku.Should().BeNull();
        }
    }
}
