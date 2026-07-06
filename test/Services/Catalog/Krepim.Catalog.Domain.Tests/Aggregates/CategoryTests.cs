using FluentAssertions;
using Krepim.Catalog.Domain.Aggregates;

namespace Krepim.Catalog.Domain.Tests.Aggregates
{
    public class CategoryTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Create_Should_ReturnFailure_WhenNameIsInvalid(string invalidName)
        {
            // Act
            var result = Category.Create(invalidName!, "Описание");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Category.EmptyName");
        }

        [Fact]
        public void Create_Should_ReturnSuccess_WhenDataIsValid()
        {
            // Act
            var result = Category.Create("Инструменты", "Все для стройки");

            // Assert
            result.IsSuccess.Should().BeTrue();
            var category = result.Value;
            category.Name.Should().Be("Инструменты");
            category.IsActive.Should().BeTrue();
            category.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void Update_Should_ModifyProperties()
        {
            // Arrange
            var category = Category.Create("Старое имя", "Старое описание").Value;

            // Act
            category.Update("Новое имя", "Новое описание");

            // Assert
            category.Name.Should().Be("Новое имя");
            category.Description.Should().Be("Новое описание");
        }

        [Fact]
        public void Lifecycle_Methods_Should_ChangeStatus()
        {
            // Arrange
            var category = Category.Create("Инструменты", "Описание").Value;

            // Act & Assert
            category.Deactivate();
            category.IsActive.Should().BeFalse();

            category.Activate();
            category.IsActive.Should().BeTrue();

            category.Delete();
            category.IsDeleted.Should().BeTrue();
            category.IsActive.Should().BeFalse();
        }
    }
}
