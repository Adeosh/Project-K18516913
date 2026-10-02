using Krepim.Catalog.Application.Features.GetProduct;
using Krepim.Catalog.Application.Features.SearchProduct;
using Krepim.SharedKernel.Results.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Krepim.Catalog.Api.Endpoints.Products
{
    internal static class ClientEndpoints
    {
        public static void MapClientEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/public")
                               .AddEndpointFilter<ResultEndpointFilter>();

            group.MapGet("/search", async (
                [FromQuery] string? term, 
                [FromQuery] int? page,
                [FromQuery] Guid? categoryId,
                [FromServices] ISender sender, 
                CancellationToken ct) =>
            {
                return await sender.Send(new SearchProductsQuery(
                    SearchTerm: term ?? string.Empty,
                    OnlyActive: true,
                    Page: page ?? 1,
                    PageSize: 20,
                    CategoryId: categoryId), ct);
            })
            .RequireRateLimiting("PublicSearchLimit");

            group.MapGet("/{id:guid}", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
            {
                return await sender.Send(new GetProductByIdQuery(id, OnlyActive: true), ct);
            });
        }
    }
}
