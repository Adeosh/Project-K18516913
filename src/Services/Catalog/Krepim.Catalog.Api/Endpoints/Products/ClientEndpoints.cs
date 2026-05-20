using Krepim.Catalog.Application.Features.GetProduct;
using Krepim.Catalog.Application.Features.SearchProduct;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Krepim.Catalog.Api.Endpoints.Products
{
    internal static class ClientEndpoints
    {
        public static void MapClientEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/public");

            group.MapGet("/search", async ([FromQuery] string term, [FromQuery] int page, [FromServices] ISender sender, CancellationToken ct)
                => await sender.Send(new SearchProductsQuery(term, OnlyActive: true, page, 20), ct));

            group.MapGet("/{id:guid}", async (Guid id, [FromServices] ISender sender, CancellationToken ct)
                => await sender.Send(new GetProductByIdQuery(id), ct));
        }
    }
}
