using Krepim.Catalog.Application.Models;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.GetCategories
{
    public record GetCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryModel>>>;
}
