using FluentAssertions;
using Krepim.Catalog.Application.Features.GetCategories;
using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models.DTOs;
using NSubstitute;

namespace Krepim.Catalog.Application.Tests.Features.Categories
{
    public class GetCategoriesQueryHandlerTests
    {
        private readonly ICategoryReadRepository _readRepository;
        private readonly GetCategoriesQueryHandler _handler;

        public GetCategoriesQueryHandlerTests()
        {
            _readRepository = Substitute.For<ICategoryReadRepository>();
            _handler = new GetCategoriesQueryHandler(_readRepository);
        }

        [Fact]
        public async Task Handle_Should_ReturnMappedCategories_WhenCategoriesExist()
        {
            // Arrange
            var categories = new List<CategoryDto>
            {
                new(Guid.NewGuid(), "Инструменты", "Ну да, они самые"),
                new(Guid.NewGuid(), "Расходники", "Ну да, те самые")
            };

            _readRepository.GetAllActiveAsync(Arg.Any<CancellationToken>())
                .Returns(categories);

            var query = new GetCategoriesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
            result.Value.Should().BeEquivalentTo(categories);

            await _readRepository.Received(1).GetAllActiveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_WhenNoCategoriesExist()
        {
            // Arrange
            _readRepository.GetAllActiveAsync(Arg.Any<CancellationToken>())
                .Returns(new List<CategoryDto>());

            var query = new GetCategoriesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }
    }
}
