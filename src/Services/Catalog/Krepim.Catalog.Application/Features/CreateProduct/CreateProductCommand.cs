using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.CreateProduct
{
    public sealed record CreateProductCommand(
        string Name, 
        string Description, 
        string Sku, 
        decimal Price, 
        Guid CategoryId) : IRequest<Result<Guid>>;
}
