using Krepim.Catalog.Application.Features.GetProduct;
using Krepim.Catalog.Application.Features.SearchProduct;
using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features
{
    internal sealed class ProductQueriesHandler(IProductReadRepository readRepo) :
        IRequestHandler<GetProductByIdQuery, Result<ProductReadModel>>,
        IRequestHandler<SearchProductsQuery, Result<IReadOnlyList<ProductReadModel>>>
    {
        public async Task<Result<ProductReadModel>> Handle(GetProductByIdQuery request, CancellationToken ct)
        {
            var product = await readRepo.GetByIdAsync(request.Id, ct);
            return product is null
                ? Result<ProductReadModel>.Failure(new Error("Product.NotFound", "Not found", ErrorType.NotFound))
                : product;
        }

        public async Task<Result<IReadOnlyList<ProductReadModel>>> Handle(SearchProductsQuery request, CancellationToken ct)
        {
            var products = await readRepo.SearchAsync(request.SearchTerm, request.OnlyActive, request.Page, request.PageSize, ct);
            return products.ToList();
        }
    }
}
