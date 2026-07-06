using FluentAssertions;
using Krepim.Catalog.Application.Features.CreateCategory;
using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.SharedKernel.Domain.Abstractions;
using NSubstitute;

namespace Krepim.Catalog.Application.Tests.Features.Categories
{
    public class CreateCategoryCommandHandlerTests
    {
        private readonly ICategoryWriteRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly CreateCategoryCommandHandler _handler;

        public CreateCategoryCommandHandlerTests()
        {
            _repository = Substitute.For<ICategoryWriteRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _handler = new CreateCategoryCommandHandler(_repository, _unitOfWork);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenCategoryCreationFails()
        {
            // Arrange
            var command = new CreateCategoryCommand("", "Описание"); // Пустое имя

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Category.EmptyName");

            await _repository.DidNotReceive().AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_AddCategoryAndSave_WhenDataIsValid()
        {
            // Arrange
            var command = new CreateCategoryCommand("Инструменты", "Описание");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();

            await _repository.Received(1).AddAsync(
                Arg.Is<Category>(c => c.Name == "Инструменты"),
                Arg.Any<CancellationToken>());

            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
