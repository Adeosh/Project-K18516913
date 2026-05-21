using Krepim.Catalog.Application.Features.GetProduct;
using Krepim.Catalog.Application.Features.SearchProduct;
using Krepim.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Krepim.Catalog.Api.Endpoints.Products
{
    internal static class ClientEndpoints
    {
        public static void MapClientEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/public");

            group.MapGet("/search", async ([FromQuery] string term, [FromQuery] int page, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new SearchProductsQuery(term, OnlyActive: true, page, 20), ct);
                return result.Match(products => Microsoft.AspNetCore.Http.Results.Ok(products));
            });

            group.MapGet("/{id:guid}", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetProductByIdQuery(id), ct);
                return result.Match(product => Microsoft.AspNetCore.Http.Results.Ok(product));
            });
        }
    }
}
