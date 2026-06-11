using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.GetCategories
{
    internal sealed class GetCategoriesQueryHandler(ICategoryReadRepository readRepository)
        : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryModel>>>
    {
        public async Task<Result<IReadOnlyList<CategoryModel>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await readRepository.GetAllActiveAsync(cancellationToken);
            return categories.ToList();
        }
    }
}
