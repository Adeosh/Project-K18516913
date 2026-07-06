using FluentAssertions;
using Krepim.Catalog.Application.Features.DeleteCategory;
using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.SharedKernel.Domain.Abstractions;
using NSubstitute;

namespace Krepim.Catalog.Application.Tests.Features.Categories
{
    public class DeleteCategoryCommandHandlerTests
    {
        private readonly ICategoryWriteRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DeleteCategoryCommandHandler _handler;

        public DeleteCategoryCommandHandlerTests()
        {
            _repository = Substitute.For<ICategoryWriteRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _handler = new DeleteCategoryCommandHandler(_repository, _unitOfWork);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenCategoryNotFound()
        {
            // Arrange
            var command = new DeleteCategoryCommand(Guid.NewGuid());
            _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Category?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Category.NotFound");
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenCategoryAlreadyDeleted()
        {
            // Arrange
            var category = Category.Create("Имя", "Описание").Value;
            category.Delete();

            var command = new DeleteCategoryCommand(category.Id);
            _repository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Category.NotFound");
        }

        [Fact]
        public async Task Handle_Should_DeleteCategoryAndSave_WhenExists()
        {
            // Arrange
            var category = Category.Create("Имя", "Описание").Value;
            var command = new DeleteCategoryCommand(category.Id);

            _repository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            category.IsDeleted.Should().BeTrue();

            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
