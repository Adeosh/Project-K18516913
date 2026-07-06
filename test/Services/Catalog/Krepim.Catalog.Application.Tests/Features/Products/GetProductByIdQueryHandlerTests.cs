using FluentAssertions;
using Krepim.Catalog.Application.Features.GetProduct;
using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models.DTOs;
using Krepim.Catalog.Domain.Enums;
using Krepim.SharedKernel.Results;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace Krepim.Catalog.Application.Tests.Features.Products
{
    public class GetProductByIdQueryHandlerTests
    {
        private readonly IProductReadRepository _readRepositoryMock;
        private readonly GetProductByIdQueryHandler _handler;

        public GetProductByIdQueryHandlerTests()
        {
            _readRepositoryMock = Substitute.For<IProductReadRepository>();
            _handler = new GetProductByIdQueryHandler(_readRepositoryMock);
        }

        [Fact]
        public async Task Handle_Should_ReturnProductDto_WhenProductExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var query = new GetProductByIdQuery(productId);

            var expectedDto = new ProductReadDto(
                Id: productId,
                Name: "Заклепка резьбовая",
                Description: "Заклепка резьбовая цилиндрическая",
                Sku: "ZRM-M6X15-ST-ZN",
                Price: 4.50m,
                Currency: "RUB",
                CategoryId: Guid.NewGuid(),
                IsActive: true,
                ImageUrls: new List<string>(),
                Standard: "DIN 7338",
                SalesUnit: (int)SalesUnit.Pcs,
                SalesStep: 250m,
                Attributes: new Dictionary<string, string>(),
                PriceTiers: new List<PriceTierDto>()
            );

            _readRepositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<ProductReadDto?>(expectedDto));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFoundFailure_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var query = new GetProductByIdQuery(productId);

            _readRepositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<ProductReadDto?>(null));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
            result.Error.Code.Should().Be("Product.NotFound");
            result.Error.Type.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task Handle_Should_ReturnProductDto_WithAllProperties_WhenProductHasFullData()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var query = new GetProductByIdQuery(productId);
            var categoryId = Guid.NewGuid();

            var attributes = new Dictionary<string, string>
            {
                { "Длина (L)", "15 мм" },
                { "Материал", "Оцинкованная сталь" }
            };

            var priceTiers = new List<PriceTierDto>
            {
                new PriceTierDto(1000, 3.8m, "RUB"),
                new PriceTierDto(5000, 3.1m, "RUB")
            };

            var imageUrls = new List<string>
            {
                "http://localhost:9000/catalog-images/image1.jpg",
                "http://localhost:9000/catalog-images/image2.jpg"
            };

            var expectedDto = new ProductReadDto(
                Id: productId,
                Name: "Болт высокопрочный",
                Description: "Болт высокопрочный с шестигранной головкой",
                Sku: "BOLT-M12X30-10.9",
                Price: 12.50m,
                Currency: "RUB",
                CategoryId: categoryId,
                IsActive: true,
                ImageUrls: imageUrls,
                Standard: "DIN 933",
                SalesUnit: (int)SalesUnit.Pcs,
                SalesStep: 100m,
                Attributes: attributes,
                PriceTiers: priceTiers
            );

            _readRepositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<ProductReadDto?>(expectedDto));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var dto = result.Value;
            dto.Should().BeEquivalentTo(expectedDto);

            dto.Attributes.Should().HaveCount(2);
            dto.Attributes.Should().ContainKey("Длина (L)").WhoseValue.Should().Be("15 мм");
            dto.Attributes.Should().ContainKey("Материал").WhoseValue.Should().Be("Оцинкованная сталь");

            dto.PriceTiers.Should().HaveCount(2);
            dto.PriceTiers.Should().Contain(t => t.MinQuantity == 1000 && t.Amount == 3.8m && t.Currency == "RUB");
            dto.PriceTiers.Should().Contain(t => t.MinQuantity == 5000 && t.Amount == 3.1m && t.Currency == "RUB");

            dto.ImageUrls.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_Should_ReturnProductDto_WhenProductIsInactive()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var query = new GetProductByIdQuery(productId);

            var expectedDto = new ProductReadDto(
                Id: productId,
                Name: "Заклепка резьбовая",
                Description: "Заклепка резьбовая цилиндрическая",
                Sku: "ZRM-M6X15-ST-ZN",
                Price: 4.50m,
                Currency: "RUB",
                CategoryId: Guid.NewGuid(),
                IsActive: false,
                ImageUrls: new List<string>(),
                Standard: "DIN 7338",
                SalesUnit: (int)SalesUnit.Pcs,
                SalesStep: 250m,
                Attributes: new Dictionary<string, string>(),
                PriceTiers: new List<PriceTierDto>()
            );

            _readRepositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<ProductReadDto?>(expectedDto));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_Should_ReturnProductDto_WithEmptyCollections_WhenProductHasNoAttributesOrImages()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var query = new GetProductByIdQuery(productId);

            var expectedDto = new ProductReadDto(
                Id: productId,
                Name: "Заклепка резьбовая",
                Description: "Заклепка резьбовая цилиндрическая",
                Sku: "ZRM-M6X15-ST-ZN",
                Price: 4.50m,
                Currency: "RUB",
                CategoryId: Guid.NewGuid(),
                IsActive: true,
                ImageUrls: new List<string>(),
                Standard: null,
                SalesUnit: (int)SalesUnit.Pcs,
                SalesStep: 250m,
                Attributes: new Dictionary<string, string>(),
                PriceTiers: new List<PriceTierDto>()
            );

            _readRepositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<ProductReadDto?>(expectedDto));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Attributes.Should().BeEmpty();
            result.Value.PriceTiers.Should().BeEmpty();
            result.Value.ImageUrls.Should().BeEmpty();
            result.Value.Standard.Should().BeNull();
        }

        [Fact]
        public async Task Handle_Should_ReturnProductDto_WithPriceTiersInCorrectOrder()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var query = new GetProductByIdQuery(productId);

            var priceTiers = new List<PriceTierDto>
            {
                new PriceTierDto(5000, 3.1m, "RUB"),
                new PriceTierDto(1000, 3.8m, "RUB"),
                new PriceTierDto(20000, 2.65m, "RUB")
            };

            var expectedDto = new ProductReadDto(
                Id: productId,
                Name: "Заклепка резьбовая",
                Description: "Заклепка резьбовая цилиндрическая",
                Sku: "ZRM-M6X15-ST-ZN",
                Price: 4.50m,
                Currency: "RUB",
                CategoryId: Guid.NewGuid(),
                IsActive: true,
                ImageUrls: new List<string>(),
                Standard: "DIN 7338",
                SalesUnit: (int)SalesUnit.Pcs,
                SalesStep: 250m,
                Attributes: new Dictionary<string, string>(),
                PriceTiers: priceTiers
            );

            _readRepositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<ProductReadDto?>(expectedDto));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            // Проверяем, что порядок такой же, как в DTO (репозиторий возвращает уже готовый DTO)
            result.Value.PriceTiers.Select(t => t.MinQuantity).Should().Equal(5000, 1000, 20000);
        }
    }
}
