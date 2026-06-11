using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.UpdateProduct
{
    public sealed record UpdateProductCommand(
        Guid Id, 
        string Name, 
        string Description, 
        Guid CategoryId,
        string[] ImageUrls) : IRequest<Result>;
}
