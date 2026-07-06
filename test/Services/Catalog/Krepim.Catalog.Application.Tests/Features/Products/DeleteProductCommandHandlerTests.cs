using FluentAssertions;
using Krepim.Catalog.Application.Features.DeleteProduct;
using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.EventBus.Events.Catalog;
using Krepim.SharedKernel.Domain.Abstractions;
using MassTransit;
using NSubstitute;

namespace Krepim.Catalog.Application.Tests.Features.Products
{
    public class DeleteProductCommandHandlerTests
    {
        private readonly IProductWriteRepository _repositoryMock;
        private readonly IPublishEndpoint _publishEndpointMock;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly DeleteProductCommandHandler _handler;

        public DeleteProductCommandHandlerTests()
        {
            _repositoryMock = Substitute.For<IProductWriteRepository>();
            _publishEndpointMock = Substitute.For<IPublishEndpoint>();
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();

            _handler = new DeleteProductCommandHandler(_repositoryMock, _publishEndpointMock, _unitOfWorkMock);
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccessAndDeleteProduct_WhenProductExistsAndIsNotDeleted()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DeleteProductCommand(productId);
            var product = ProductTestFactory.CreateActive(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            product.IsDeleted.Should().BeTrue();
            product.IsActive.Should().BeFalse();

            await _publishEndpointMock.Received(1).Publish(
                Arg.Is<ProductDeletedIntegrationEvent>(e => e.ProductId == productId),
                Arg.Any<CancellationToken>());

            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccessWithoutDeletion_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DeleteProductCommand(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(null));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            await _publishEndpointMock.Received(0).Publish(
                Arg.Any<ProductDeletedIntegrationEvent>(),
                Arg.Any<CancellationToken>());

            await _unitOfWorkMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccessWithoutDeletion_WhenProductIsAlreadyDeleted()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DeleteProductCommand(productId);
            var product = ProductTestFactory.CreateDeleted(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            product.IsDeleted.Should().BeTrue();
            product.IsActive.Should().BeFalse();

            await _publishEndpointMock.Received(0).Publish(
                Arg.Any<ProductDeletedIntegrationEvent>(),
                Arg.Any<CancellationToken>());

            await _unitOfWorkMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_DeleteProduct_WhenProductIsInactive()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DeleteProductCommand(productId);
            var product = ProductTestFactory.CreateInactive(productId);

            product.IsActive.Should().BeFalse();
            product.IsDeleted.Should().BeFalse();

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            product.IsDeleted.Should().BeTrue();
            product.IsActive.Should().BeFalse();

            await _publishEndpointMock.Received(1).Publish(
                Arg.Is<ProductDeletedIntegrationEvent>(e => e.ProductId == productId),
                Arg.Any<CancellationToken>());

            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_PublishEventBeforeSavingChanges()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DeleteProductCommand(productId);
            var product = ProductTestFactory.CreateActive(productId);

            _repositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Received.InOrder(() =>
            {
                _publishEndpointMock.Publish(
                    Arg.Is<ProductDeletedIntegrationEvent>(e => e.ProductId == productId),
                    Arg.Any<CancellationToken>());

                _unitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>());
            });
        }
    }
}
