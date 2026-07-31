using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.DeleteCategory
{
    internal sealed class DeleteCategoryCommandHandler(
        ICategoryWriteRepository categoryRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteCategoryCommand, Result>
    {
        public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken ct)
        {
            Category? category = await categoryRepository.GetByIdAsync(request.Id, ct);

            if (category is null || category.IsDeleted)
                return Result.Failure(new Error("Category.NotFound", "Категория не найдена", ErrorType.NotFound));

            category.Delete();

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
    }
}
