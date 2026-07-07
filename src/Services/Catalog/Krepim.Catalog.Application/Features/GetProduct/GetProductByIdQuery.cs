using Krepim.Catalog.Application.Models.DTOs;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.GetProduct
{
    public sealed record GetProductByIdQuery(Guid Id, bool OnlyActive = false) : IRequest<Result<ProductReadDto>>;
}
