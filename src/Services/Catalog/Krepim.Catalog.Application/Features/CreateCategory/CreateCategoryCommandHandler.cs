using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.CreateCategory
{
    internal sealed class CreateCategoryCommandHandler(
        ICategoryWriteRepository categoryRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateCategoryCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken ct)
        {
            Result<Category> result = Category.Create(request.Name, request.Description);
            if (result.IsFailure)
                return Result<Guid>.Failure(result.Error);

            await categoryRepository.AddAsync(result.Value, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return result.Value.Id;
        }
    }
}
