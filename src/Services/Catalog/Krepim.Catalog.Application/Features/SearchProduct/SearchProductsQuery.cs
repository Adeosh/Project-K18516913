using Krepim.Catalog.Application.Models.DTOs;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.SearchProduct
{
    public sealed record SearchProductsQuery(
        string SearchTerm, 
        bool OnlyActive, 
        int Page = 1, 
        int PageSize = 20) : IRequest<Result<IReadOnlyList<ProductReadDto>>>;
}
