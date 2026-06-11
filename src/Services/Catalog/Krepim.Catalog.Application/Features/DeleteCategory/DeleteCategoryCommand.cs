using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.DeleteCategory
{
    public record DeleteCategoryCommand(Guid Id) : IRequest<Result>;
}
