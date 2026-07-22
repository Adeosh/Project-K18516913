using FluentAssertions;
using Krepim.Catalog.Application.Features.SearchProduct;
using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models.DTOs;
using Krepim.Catalog.Domain.Enums;
using NSubstitute;

namespace Krepim.Catalog.Application.Tests.Features.Products
{
    public class SearchProductsQueryHandlerTests
    {
        private readonly IProductReadRepository _readRepositoryMock;
        private readonly SearchProductsQueryHandler _handler;

        public SearchProductsQueryHandlerTests()
        {
            _readRepositoryMock = Substitute.For<IProductReadRepository>();
            _handler = new SearchProductsQueryHandler(_readRepositoryMock);
        }

        [Fact]
        public async Task Handle_Should_ReturnProducts_WhenSearchReturnsResults()
        {
            // Arrange
            var searchTerm = "заклепка";
            var onlyActive = true;
            var page = 1;
            var pageSize = 10;
            var query = new SearchProductsQuery(searchTerm, onlyActive, page, pageSize);

            var expectedProducts = new List<ProductReadDto>
            {
                ProductTestFactory.CreateProductDto(Guid.NewGuid(), "Заклепка резьбовая М6", "ZRM-M6", true),
                ProductTestFactory.CreateProductDto(Guid.NewGuid(), "Заклепка вытяжная", "ZRM-M8", true)
            };

            _readRepositoryMock.SearchAsync(searchTerm, onlyActive, page, pageSize, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<ProductReadDto>>(expectedProducts));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().HaveCount(2);
            result.Value.Should().BeEquivalentTo(expectedProducts);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_WhenSearchReturnsNoResults()
        {
            // Arrange
            var searchTerm = "несуществующий";
            var onlyActive = true;
            var page = 1;
            var pageSize = 10;
            var query = new SearchProductsQuery(searchTerm, onlyActive, page, pageSize);

            var emptyList = new List<ProductReadDto>();

            _readRepositoryMock.SearchAsync(searchTerm, onlyActive, page, pageSize, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<ProductReadDto>>(emptyList));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_Should_SearchOnlyActiveProducts_WhenOnlyActiveIsTrue()
        {
            // Arrange
            var searchTerm = "болт";
            var onlyActive = true;
            var page = 1;
            var pageSize = 10;
            var query = new SearchProductsQuery(searchTerm, onlyActive, page, pageSize);

            var activeProducts = new List<ProductReadDto>
            {
                ProductTestFactory.CreateProductDto(Guid.NewGuid(), "Болт высокопрочный", "BOLT-M12", true),
                ProductTestFactory.CreateProductDto(Guid.NewGuid(), "Болт DIN 933", "BOLT-DIN933", true)
            };

            _readRepositoryMock.SearchAsync(searchTerm, onlyActive, page, pageSize, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<ProductReadDto>>(activeProducts));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().AllSatisfy(p => p.IsActive.Should().BeTrue());
            await _readRepositoryMock.Received(1).SearchAsync(searchTerm, true, page, pageSize, Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_SearchAllProducts_WhenOnlyActiveIsFalse()
        {
            // Arrange
            var searchTerm = "болт";
            var onlyActive = false;
            var page = 1;
            var pageSize = 10;
            var query = new SearchProductsQuery(searchTerm, onlyActive, page, pageSize);

            var allProducts = new List<ProductReadDto>
            {
                ProductTestFactory.CreateProductDto(Guid.NewGuid(), "Болт высокопрочный", "BOLT-M12", true),
                ProductTestFactory.CreateProductDto(Guid.NewGuid(), "Болт неактивный", "BOLT-OLD", false),
                ProductTestFactory.CreateProductDto(Guid.NewGuid(), "Болт удаленный", "BOLT-DEL", false)
            };

            _readRepositoryMock.SearchAsync(searchTerm, onlyActive, page, pageSize, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<ProductReadDto>>(allProducts));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(3);
            await _readRepositoryMock.Received(1).SearchAsync(searchTerm, false, page, pageSize, Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_ReturnPagedResults_WhenPageAndPageSizeAreSpecified()
        {
            // Arrange
            var searchTerm = "винт";
            var onlyActive = true;
            var page = 2;
            var pageSize = 5;
            var query = new SearchProductsQuery(searchTerm, onlyActive, page, pageSize);

            var pagedProducts = new List<ProductReadDto>
            {
                ProductTestFactory.CreateProductDto(Guid.NewGuid(), "Винт М6", "SCR-M6", true),
                ProductTestFactory.CreateProductDto(Guid.NewGuid(), "Винт М8", "SCR-M8", true),
                ProductTestFactory.CreateProductDto(Guid.NewGuid(), "Винт М10", "SCR-M10", true)
            };

            _readRepositoryMock.SearchAsync(searchTerm, onlyActive, page, pageSize, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<ProductReadDto>>(pagedProducts));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(3);
            await _readRepositoryMock.Received(1).SearchAsync(searchTerm, onlyActive, 2, 5, Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_ReturnProducts_WithFullData_WhenSearchReturnsRichResults()
        {
            // Arrange
            var searchTerm = "заклепка";
            var onlyActive = true;
            var page = 1;
            var pageSize = 10;
            var query = new SearchProductsQuery(searchTerm, onlyActive, page, pageSize);

            var categoryId = Guid.NewGuid();
            var attributes = new Dictionary<string, string>
            {
                { "Длина", "15 мм" },
                { "Материал", "Сталь" }
            };
            var priceTiers = new List<PriceTierDto>
            {
                new PriceTierDto(1000, 3.8m, "RUB"),
                new PriceTierDto(5000, 3.1m, "RUB")
            };
            var imageUrls = new List<string>
            {
                "http://localhost:9000/image1.jpg",
                "http://localhost:9000/image2.jpg"
            };

            var expectedProduct = new ProductReadDto(
                Id: Guid.NewGuid(),
                Name: "Заклепка резьбовая",
                Description: "Заклепка резьбовая цилиндрическая",
                Sku: "ZRM-M6X15-ST-ZN",
                Price: 4.50m,
                Currency: "RUB",
                CategoryId: categoryId,
                IsActive: true,
                ImageUrls: imageUrls,
                Standard: "DIN 7338",
                SalesUnit: (int)SalesUnit.Pcs,
                SalesStep: 250m,
                Attributes: attributes,
                PriceTiers: priceTiers
            );

            _readRepositoryMock.SearchAsync(searchTerm, onlyActive, page, pageSize, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<ProductReadDto>>(new List<ProductReadDto> { expectedProduct }));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var product = result.Value.First();
            product.Should().BeEquivalentTo(expectedProduct);
            product.Attributes.Should().HaveCount(2);
            product.PriceTiers.Should().HaveCount(2);
            product.ImageUrls.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_Should_SearchWithEmptySearchTerm_WhenSearchTermIsNullOrEmpty()
        {
            // Arrange
            var searchTerm = string.Empty;
            var onlyActive = true;
            var page = 1;
            var pageSize = 10;
            var query = new SearchProductsQuery(searchTerm, onlyActive, page, pageSize);

            var products = new List<ProductReadDto>
            {
                ProductTestFactory.CreateProductDto(Guid.NewGuid(), "Продукт 1", "SKU1", true),
                ProductTestFactory.CreateProductDto(Guid.NewGuid(), "Продукт 2", "SKU2", true)
            };

            _readRepositoryMock.SearchAsync(searchTerm, onlyActive, page, pageSize, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<ProductReadDto>>(products));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
            await _readRepositoryMock.Received(1).SearchAsync("", true, page, pageSize, Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccess_WithEmptyList_WhenSearchTermIsNull()
        {
            // Arrange
            var query = new SearchProductsQuery(string.Empty, true, 1, 10);

            _readRepositoryMock.SearchAsync(string.Empty, true, 1, 10, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<ProductReadDto>>(new List<ProductReadDto>()));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_Should_CallRepositoryWithCorrectParameters()
        {
            // Arrange
            var searchTerm = "шуруп";
            var onlyActive = false;
            var page = 3;
            var pageSize = 20;
            var categoryId = Arg.Any<Guid?>();
            var query = new SearchProductsQuery(searchTerm, onlyActive, page, pageSize);

            _readRepositoryMock.SearchAsync(searchTerm, onlyActive, page, pageSize, categoryId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<ProductReadDto>>(new List<ProductReadDto>()));

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            await _readRepositoryMock.Received(1).SearchAsync(
                Arg.Is<string>(s => s == searchTerm),
                Arg.Is<bool>(b => b == onlyActive),
                Arg.Is<int>(i => i == page),
                Arg.Is<int>(i => i == pageSize),
                categoryId,
                Arg.Any<CancellationToken>());
        }
    }
}
