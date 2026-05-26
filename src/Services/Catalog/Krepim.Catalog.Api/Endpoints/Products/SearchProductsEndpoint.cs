using Krepim.Catalog.Application.Features.SearchProduct;
using Krepim.Catalog.Application.Models;
using Krepim.SharedKernel.Results.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Krepim.Catalog.Api.Endpoints.Products
{
    internal static class SearchProductsEndpoint
    {
        public static void MapSearchProductsEndpoint(this IEndpointRouteBuilder builder)
        {
            builder.MapGet("/search", async (
                [FromQuery] string searchTerm,
                [FromQuery] int page,
                [FromQuery] int pageSize,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new SearchProductsQuery(searchTerm, true, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize);

                return await sender.Send(query, ct);
            })
            .AddEndpointFilter<ResultEndpointFilter>()
            .WithName("SearchProducts")
            .WithSummary("Быстрый поиск товаров (Read Model / MongoDB)")
            .Produces<IReadOnlyList<ProductReadModel>>(StatusCodes.Status200OK);
        }
    }
}