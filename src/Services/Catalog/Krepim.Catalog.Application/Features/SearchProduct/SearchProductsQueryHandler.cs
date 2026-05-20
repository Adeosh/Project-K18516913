using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.SearchProduct
{
    internal sealed class SearchProductsQueryHandler(IProductReadRepository readRepository) : IRequestHandler<SearchProductsQuery, Result<IReadOnlyList<ProductReadModel>>>
    {
        public async Task<Result<IReadOnlyList<ProductReadModel>>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await readRepository.SearchAsync(request.SearchTerm, request.OnlyActive, request.Page, request.PageSize, cancellationToken);

            return products.ToList();
        }
    }
}
