using FluentAssertions;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.Catalog.Domain.Enums;

namespace Krepim.Catalog.Domain.Tests.Aggregates
{
    public class ProductTests
    {
        [Fact]
        public void Create_Should_ReturnFailure_WhenPriceIsInvalid()
        {
            // Act
            var result = Product.Create("Болт", "Описание", "SKU-123", -10m, Guid.NewGuid(), null, SalesUnit.Pcs, 1);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Product.InvalidPrice");
        }

        [Fact]
        public void Create_Should_ReturnSuccess_WhenDataIsValid()
        {
            // Act
            var categoryId = Guid.NewGuid();
            var result = Product.Create("Болт", "Описание", "SKU-123", 100m, categoryId, "DIN 933", SalesUnit.Pcs, 1);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var product = result.Value;
            product.Name.Should().Be("Болт");
            product.IsActive.Should().BeFalse();
            product.SalesStep.Should().Be(1);
        }

        [Fact]
        public void UpdateDetails_Should_UpdatePropertiesCorrectly()
        {
            // Arrange
            var product = Product.Create("Старое имя", "Старое", "SKU-1", 10m, Guid.NewGuid(), null, SalesUnit.Pcs, 1).Value;
            var newCategory = Guid.NewGuid();

            // Act
            product.UpdateDetails("Новое имя", "Новое", newCategory, "ГОСТ", SalesUnit.Kg, 0.5m);

            // Assert
            product.Name.Should().Be("Новое имя");
            product.Standard.Should().Be("ГОСТ");
            product.SalesUnit.Should().Be(SalesUnit.Kg);
            product.SalesStep.Should().Be(0.5m);
        }

        [Fact]
        public void SetAttributes_Should_ReplaceExistingAttributes()
        {
            // Arrange
            var product = Product.Create("Болт", "Описание", "SKU-1", 10m, Guid.NewGuid(), null, SalesUnit.Pcs, 1).Value;
            var attrs = new Dictionary<string, string> { { "Материал", "Сталь" } };

            // Act
            product.SetAttributes(attrs);

            // Assert
            product.Attributes.Should().ContainKey("Материал").WhoseValue.Should().Be("Сталь");
            product.Attributes.Should().HaveCount(1);
        }

        [Fact]
        public void ImageManagement_Should_WorkCorrectly()
        {
            // Arrange
            var product = Product.Create("Болт", "Описание", "SKU-1", 10m, Guid.NewGuid(), null, SalesUnit.Pcs, 1).Value;
            var url = "http://image.url";

            // Act
            product.AddImage(url);
            product.AddImage(url);
            product.RemoveImage(url);

            // Assert
            product.ImageUrls.Should().NotContain(url);
        }

        [Fact]
        public void Lifecycle_Methods_Should_ChangeStatus()
        {
            // Arrange
            var product = Product.Create("Болт", "Описание", "SKU-1", 10m, Guid.NewGuid(), null, SalesUnit.Pcs, 1).Value;

            // Act & Assert
            product.Publish();
            product.IsActive.Should().BeTrue();

            product.Deactivate();
            product.IsActive.Should().BeFalse();

            product.Delete();
            product.IsDeleted.Should().BeTrue();
            product.IsActive.Should().BeFalse();
        }
    }
}
