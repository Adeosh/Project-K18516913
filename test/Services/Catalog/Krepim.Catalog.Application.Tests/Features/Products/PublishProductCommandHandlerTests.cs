using FluentAssertions;
using Krepim.Catalog.Application.Features.PublishProduct;
using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.EventBus.Events.Catalog;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
using MassTransit;
using NSubstitute;

namespace Krepim.Catalog.Application.Tests.Features.Products
{
    public class PublishProductCommandHandlerTests
    {
        private readonly IProductWriteRepository _repositoryMock;
        private readonly IPublishEndpoint _publishEndpointMock;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly PublishProductCommandHandler _handler;

        public PublishProductCommandHandlerTests()
        {
            _repositoryMock = Substitute.For<IProductWriteRepository>();
            _publishEndpointMock = Substitute.For<IPublishEndpoint>();
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();

            _handler = new PublishProductCommandHandler(_repositoryMock, _publishEndpointMock, _unitOfWorkMock);
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccessAndPublishEvent_WhenProductExistsAndIsNotDeleted()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new PublishProductCommand(productId);
            var product = ProductTestFactory.CreateInactive(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            product.IsActive.Should().BeTrue();

            await _publishEndpointMock.Received(1).Publish(
                Arg.Is<ProductStatusChangedIntegrationEvent>(e =>
                    e.ProductId == productId &&
                    e.IsActive == true),
                Arg.Any<CancellationToken>());

            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFoundFailure_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new PublishProductCommand(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(null));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
            result.Error.Code.Should().Be("Product.NotFound");
            result.Error.Type.Should().Be(ErrorType.NotFound);

            await _publishEndpointMock.Received(0).Publish(
                Arg.Any<ProductStatusChangedIntegrationEvent>(),
                Arg.Any<CancellationToken>());

            await _unitOfWorkMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFoundFailure_WhenProductIsDeleted()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new PublishProductCommand(productId);
            var product = ProductTestFactory.CreateDeleted(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
            result.Error.Code.Should().Be("Product.NotFound");
            result.Error.Type.Should().Be(ErrorType.NotFound);

            product.IsDeleted.Should().BeTrue();
            product.IsActive.Should().BeFalse();

            await _publishEndpointMock.Received(0).Publish(
                Arg.Any<ProductStatusChangedIntegrationEvent>(),
                Arg.Any<CancellationToken>());

            await _unitOfWorkMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_PublishProduct_WhenProductIsInactive()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new PublishProductCommand(productId);
            var product = ProductTestFactory.CreateInactive(productId);

            product.IsActive.Should().BeFalse();
            product.IsDeleted.Should().BeFalse();

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            product.IsActive.Should().BeTrue();
            product.IsDeleted.Should().BeFalse();

            await _publishEndpointMock.Received(1).Publish(
                Arg.Is<ProductStatusChangedIntegrationEvent>(e =>
                    e.ProductId == productId &&
                    e.IsActive == true),
                Arg.Any<CancellationToken>());

            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_KeepProductActive_WhenProductIsAlreadyActive()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new PublishProductCommand(productId);
            var product = ProductTestFactory.CreateActive(productId);

            product.IsActive.Should().BeTrue();
            product.IsDeleted.Should().BeFalse();

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            product.IsActive.Should().BeTrue();
            product.IsDeleted.Should().BeFalse();

            await _publishEndpointMock.Received(1).Publish(
                Arg.Is<ProductStatusChangedIntegrationEvent>(e =>
                    e.ProductId == productId &&
                    e.IsActive == true),
                Arg.Any<CancellationToken>());

            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_PublishEventBeforeSavingChanges()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new PublishProductCommand(productId);
            var product = ProductTestFactory.CreateInactive(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Received.InOrder(() =>
            {
                _publishEndpointMock.Publish(
                    Arg.Is<ProductStatusChangedIntegrationEvent>(e =>
                        e.ProductId == productId &&
                        e.IsActive == true),
                    Arg.Any<CancellationToken>());

                _unitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>());
            });
        }

        [Fact]
        public async Task Handle_Should_NotCallPublish_WhenProductNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new PublishProductCommand(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(null));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();

            await _publishEndpointMock.Received(0).Publish(
                Arg.Any<ProductStatusChangedIntegrationEvent>(),
                Arg.Any<CancellationToken>());

            await _unitOfWorkMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenProductIsDeleted_EvenIfCalledWithPublish()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new PublishProductCommand(productId);
            var product = ProductTestFactory.CreateDeleted(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Product.NotFound");

            product.IsActive.Should().BeFalse();
            product.IsDeleted.Should().BeTrue();

            await _publishEndpointMock.Received(0).Publish(
                Arg.Any<ProductStatusChangedIntegrationEvent>(),
                Arg.Any<CancellationToken>());

            await _unitOfWorkMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
