using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models.DTOs;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.GetProduct
{
    internal sealed class GetProductByIdQueryHandler(IProductReadRepository readRepo)
        : IRequestHandler<GetProductByIdQuery, Result<ProductReadDto>>
    {
        public async Task<Result<ProductReadDto>> Handle(GetProductByIdQuery request, CancellationToken ct)
        {
            var product = await readRepo.GetByIdAsync(request.Id, ct);
            return product is null
                ? Result<ProductReadDto>.Failure(new Error("Product.NotFound", "Not found", ErrorType.NotFound))
                : product;
        }
    }
}
