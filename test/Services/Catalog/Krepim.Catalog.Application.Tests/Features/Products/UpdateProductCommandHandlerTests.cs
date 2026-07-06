using FluentAssertions;
using Krepim.Catalog.Application.Features.UpdateProduct;
using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models.DTOs;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.Catalog.Domain.Enums;
using Krepim.EventBus.Events.Catalog;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
using Krepim.SharedKernel.ValueObjects;
using MassTransit;
using NSubstitute;

namespace Krepim.Catalog.Application.Tests.Features.Products
{
    public class UpdateProductCommandHandlerTests
    {
        private readonly IProductWriteRepository _repositoryMock;
        private readonly IPublishEndpoint _publishEndpointMock;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly UpdateProductCommandHandler _handler;

        public UpdateProductCommandHandlerTests()
        {
            _repositoryMock = Substitute.For<IProductWriteRepository>();
            _publishEndpointMock = Substitute.For<IPublishEndpoint>();
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();

            _handler = new UpdateProductCommandHandler(_repositoryMock, _publishEndpointMock, _unitOfWorkMock);
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccessAndUpdateProduct_WhenProductExistsAndIsNotDeleted()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var command = new UpdateProductCommand(
                Id: productId,
                Name: "Болт обновленный",
                Description: "Болт обновленный с шестигранной головкой",
                CategoryId: categoryId,
                ImageUrls: new[] { "http://localhost:9000/image1.jpg" },
                Standard: "DIN 933",
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 50m,
                Attributes: new Dictionary<string, string>
                {
                    { "Длина", "30 мм" },
                    { "Материал", "Сталь" }
                },
                PriceTiers: new List<PriceTierDto>
                {
                    new PriceTierDto(1000, 12.5m, "RUB"),
                    new PriceTierDto(5000, 10.2m, "RUB")
                }
            );

            var product = ProductTestFactory.CreateActive(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            product.Name.Should().Be("Болт обновленный");
            product.Description.Should().Be("Болт обновленный с шестигранной головкой");
            product.CategoryId.Should().Be(categoryId);
            product.Standard.Should().Be("DIN 933");
            product.SalesUnit.Should().Be(SalesUnit.Pcs);
            product.SalesStep.Should().Be(50m);
            product.ImageUrls.Should().Contain("http://localhost:9000/image1.jpg");
            product.Attributes.Should().HaveCount(2);
            product.PriceTiers.Should().HaveCount(2);

            await _publishEndpointMock.Received(1).Publish(
                Arg.Is<ProductUpdatedIntegrationEvent>(e => e.ProductId == productId),
                Arg.Any<CancellationToken>());

            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFoundFailure_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new UpdateProductCommand(
                Id: productId,
                Name: "Болт",
                Description: "Болт",
                CategoryId: Guid.NewGuid(),
                ImageUrls: null,
                Standard: null,
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 1m,
                Attributes: null,
                PriceTiers: null
            );

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(null));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Product.NotFound");
            result.Error.Type.Should().Be(ErrorType.NotFound);

            await _publishEndpointMock.Received(0).Publish(
                Arg.Any<ProductUpdatedIntegrationEvent>(),
                Arg.Any<CancellationToken>());

            await _unitOfWorkMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFoundFailure_WhenProductIsDeleted()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new UpdateProductCommand(
                Id: productId,
                Name: "Болт",
                Description: "Болт",
                CategoryId: Guid.NewGuid(),
                ImageUrls: null,
                Standard: null,
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 1m,
                Attributes: null,
                PriceTiers: null
            );

            var product = ProductTestFactory.CreateDeleted(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Product.NotFound");

            await _publishEndpointMock.Received(0).Publish(
                Arg.Any<ProductUpdatedIntegrationEvent>(),
                Arg.Any<CancellationToken>());

            await _unitOfWorkMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_UpdateWithEmptyImageUrls_WhenImageUrlsIsNull()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new UpdateProductCommand(
                Id: productId,
                Name: "Болт",
                Description: "Болт",
                CategoryId: Guid.NewGuid(),
                ImageUrls: null,
                Standard: null,
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 1m,
                Attributes: null,
                PriceTiers: null
            );

            var product = ProductTestFactory.CreateActive(productId);
            product.SetImages(new[] { "http://localhost:9000/old-image.jpg" });

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            product.ImageUrls.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_Should_NotUpdateAttributes_WhenAttributesIsNull()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new UpdateProductCommand(
                Id: productId,
                Name: "Болт",
                Description: "Болт",
                CategoryId: Guid.NewGuid(),
                ImageUrls: null,
                Standard: null,
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 1m,
                Attributes: null,
                PriceTiers: null
            );

            var product = ProductTestFactory.CreateActive(productId);
            var originalAttributes = new Dictionary<string, string> { { "Существующий", "Атрибут" } };
            product.SetAttributes(originalAttributes);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            product.Attributes.Should().BeEquivalentTo(originalAttributes);
        }

        [Fact]
        public async Task Handle_Should_NotUpdatePriceTiers_WhenPriceTiersIsNull()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new UpdateProductCommand(
                Id: productId,
                Name: "Болт",
                Description: "Болт",
                CategoryId: Guid.NewGuid(),
                ImageUrls: null,
                Standard: null,
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 1m,
                Attributes: null,
                PriceTiers: null
            );

            var product = ProductTestFactory.CreateActive(productId);
            var originalTiers = new[] { new PriceTier(100, Money.Rubles(9.0m)) };
            product.UpdatePriceTiers(originalTiers);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            product.PriceTiers.Should().HaveCount(1);
            product.PriceTiers.First().MinQuantity.Should().Be(100);
        }

        [Fact]
        public async Task Handle_Should_PublishEventWithCorrectData()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var imageUrls = new[] { "http://localhost:9000/image1.jpg" };
            var attributes = new Dictionary<string, string> { { "Длина", "30 мм" } };
            var priceTiers = new List<PriceTierDto>
            {
                new PriceTierDto(1000, 12.5m, "RUB")
            };

            var command = new UpdateProductCommand(
                Id: productId,
                Name: "Болт",
                Description: "Болт высокопрочный",
                CategoryId: categoryId,
                ImageUrls: imageUrls,
                Standard: "DIN 933",
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 50m,
                Attributes: attributes,
                PriceTiers: priceTiers
            );

            var product = ProductTestFactory.CreateActive(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _publishEndpointMock.Received(1).Publish(
                Arg.Is<ProductUpdatedIntegrationEvent>(e =>
                    e.ProductId == productId &&
                    e.Name == "Болт" &&
                    e.Description == "Болт высокопрочный" &&
                    e.CategoryId == categoryId &&
                    e.Standard == "DIN 933" &&
                    e.SalesUnit == (int)SalesUnit.Pcs &&
                    e.SalesStep == 50m &&
                    e.ImageUrls.Contains("http://localhost:9000/image1.jpg") &&
                    e.Attributes.ContainsKey("Длина") &&
                    e.PriceTiers.Count == 1
                ),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_PublishEventBeforeSavingChanges()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new UpdateProductCommand(
                Id: productId,
                Name: "Болт",
                Description: "Болт",
                CategoryId: Guid.NewGuid(),
                ImageUrls: null,
                Standard: null,
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 1m,
                Attributes: null,
                PriceTiers: null
            );

            var product = ProductTestFactory.CreateActive(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Received.InOrder(() =>
            {
                _publishEndpointMock.Publish(
                    Arg.Any<ProductUpdatedIntegrationEvent>(),
                    Arg.Any<CancellationToken>());

                _unitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>());
            });
        }

        [Fact]
        public async Task Handle_Should_UpdateSalesStepToDefault_WhenSalesStepIsZeroOrNegative()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new UpdateProductCommand(
                Id: productId,
                Name: "Болт",
                Description: "Болт",
                CategoryId: Guid.NewGuid(),
                ImageUrls: null,
                Standard: null,
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 0,
                Attributes: null,
                PriceTiers: null
            );

            var product = ProductTestFactory.CreateActive(productId);
            product.SalesStep.Should().Be(250m);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            product.SalesStep.Should().Be(1m);
        }

        [Fact]
        public async Task Handle_Should_UpdatePriceTiers_WhenPriceTiersAreProvided()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var priceTiers = new List<PriceTierDto>
            {
                new PriceTierDto(1000, 12.5m, "RUB"),
                new PriceTierDto(5000, 10.2m, "RUB"),
                new PriceTierDto(20000, 8.7m, "RUB")
            };

            var command = new UpdateProductCommand(
                Id: productId,
                Name: "Болт",
                Description: "Болт",
                CategoryId: Guid.NewGuid(),
                ImageUrls: null,
                Standard: null,
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 1m,
                Attributes: null,
                PriceTiers: priceTiers
            );

            var product = ProductTestFactory.CreateActive(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            product.PriceTiers.Should().HaveCount(3);
            product.PriceTiers.Should().Contain(pt => pt.MinQuantity == 1000 && pt.Price.Amount == 12.5m);
            product.PriceTiers.Should().Contain(pt => pt.MinQuantity == 5000 && pt.Price.Amount == 10.2m);
            product.PriceTiers.Should().Contain(pt => pt.MinQuantity == 20000 && pt.Price.Amount == 8.7m);
        }

        [Fact]
        public async Task Handle_Should_UpdateAttributes_WhenAttributesAreProvided()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var attributes = new Dictionary<string, string>
            {
                { "Длина (L)", "15 мм" },
                { "Материал", "Оцинкованная сталь" },
                { "Тип буртика", "Уменьшенный" }
            };

            var command = new UpdateProductCommand(
                Id: productId,
                Name: "Болт",
                Description: "Болт",
                CategoryId: Guid.NewGuid(),
                ImageUrls: null,
                Standard: null,
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 1m,
                Attributes: attributes,
                PriceTiers: null
            );

            var product = ProductTestFactory.CreateActive(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            product.Attributes.Should().HaveCount(3);
            product.Attributes.Should().ContainKey("Длина (L)").WhoseValue.Should().Be("15 мм");
            product.Attributes.Should().ContainKey("Материал").WhoseValue.Should().Be("Оцинкованная сталь");
            product.Attributes.Should().ContainKey("Тип буртика").WhoseValue.Should().Be("Уменьшенный");
        }

        [Fact]
        public async Task Handle_Should_UpdateImages_WhenImageUrlsAreProvided()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var imageUrls = new[]
            {
                "http://localhost:9000/image1.jpg",
                "http://localhost:9000/image2.jpg",
                "http://localhost:9000/image3.jpg"
            };

            var command = new UpdateProductCommand(
                Id: productId,
                Name: "Болт",
                Description: "Болт",
                CategoryId: Guid.NewGuid(),
                ImageUrls: imageUrls,
                Standard: null,
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 1m,
                Attributes: null,
                PriceTiers: null
            );

            var product = ProductTestFactory.CreateActive(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            product.ImageUrls.Should().HaveCount(3);
            product.ImageUrls.Should().Contain(imageUrls);
        }

        [Fact]
        public async Task Handle_Should_UpdateWithEmptyArray_WhenImageUrlsIsEmptyArray()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new UpdateProductCommand(
                Id: productId,
                Name: "Болт",
                Description: "Болт",
                CategoryId: Guid.NewGuid(),
                ImageUrls: Array.Empty<string>(),
                Standard: null,
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 1m,
                Attributes: null,
                PriceTiers: null
            );

            var product = ProductTestFactory.CreateActive(productId);
            product.SetImages(new[] { "http://localhost:9000/old-image.jpg" });

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            product.ImageUrls.Should().BeEmpty();
        }
    }
}
