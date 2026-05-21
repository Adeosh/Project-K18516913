using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.SearchProduct
{
    internal sealed class SearchProductsQueryHandler(IProductReadRepository readRepo)
        : IRequestHandler<SearchProductsQuery, Result<IReadOnlyList<ProductReadModel>>>
    {
        public async Task<Result<IReadOnlyList<ProductReadModel>>> Handle(SearchProductsQuery request, CancellationToken ct)
        {
            var products = await readRepo.SearchAsync(request.SearchTerm, request.OnlyActive, request.Page, request.PageSize, ct);

            return products.ToList();
        }
    }
}
