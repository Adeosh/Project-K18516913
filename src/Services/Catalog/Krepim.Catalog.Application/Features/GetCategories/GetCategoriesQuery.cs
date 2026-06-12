using Krepim.Catalog.Application.Models.DTOs;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.GetCategories
{
    public record GetCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryDto>>>;
}
