using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.PublishProduct
{
    public sealed record PublishProductCommand(Guid Id) : IRequest<Result>;
}
