using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.GetProduct
{
    internal sealed class GetProductByIdQueryHandler(IProductReadRepository readRepo)
        : IRequestHandler<GetProductByIdQuery, Result<ProductReadModel>>
    {
        public async Task<Result<ProductReadModel>> Handle(GetProductByIdQuery request, CancellationToken ct)
        {
            var product = await readRepo.GetByIdAsync(request.Id, ct);
            return product is null
                ? Result<ProductReadModel>.Failure(new Error("Product.NotFound", "Not found", ErrorType.NotFound))
                : product;
        }
    }
}
