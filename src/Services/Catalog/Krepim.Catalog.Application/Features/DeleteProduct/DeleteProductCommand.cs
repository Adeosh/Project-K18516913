using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.DeleteProduct
{
    public sealed record DeleteProductCommand(Guid Id) : IRequest<Result>;
}
