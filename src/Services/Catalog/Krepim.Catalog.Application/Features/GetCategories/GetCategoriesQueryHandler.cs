using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models.DTOs;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.GetCategories
{
    internal sealed class GetCategoriesQueryHandler(ICategoryReadRepository readRepository)
        : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
    {
        public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            IReadOnlyList<CategoryDto> categories = await readRepository.GetAllActiveAsync(cancellationToken);
            return categories.ToList();
        }
    }
}
