using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.CreateCategory
{
    public record CreateCategoryCommand(string Name, string Description) : IRequest<Result<Guid>>;
}
