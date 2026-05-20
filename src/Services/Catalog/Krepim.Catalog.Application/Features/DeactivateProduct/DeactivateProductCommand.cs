using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.DeactivateProduct
{
    public sealed record DeactivateProductCommand(Guid Id) : IRequest<Result>;
}
