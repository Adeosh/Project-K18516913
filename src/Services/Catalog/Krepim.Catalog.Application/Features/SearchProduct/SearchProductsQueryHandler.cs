using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models.DTOs;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.SearchProduct
{
    internal sealed class SearchProductsQueryHandler(IProductReadRepository readRepo)
        : IRequestHandler<SearchProductsQuery, Result<IReadOnlyList<ProductReadDto>>>
    {
        public async Task<Result<IReadOnlyList<ProductReadDto>>> Handle(SearchProductsQuery request, CancellationToken ct)
        {
            IReadOnlyList<ProductReadDto> products = await readRepo.SearchAsync(request.SearchTerm, request.OnlyActive, request.Page, request.PageSize, request.CategoryId, ct);

            return products.ToList();
        }
    }
}
