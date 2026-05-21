using Krepim.Catalog.Application.Features.DeactivateProduct;
using Krepim.Catalog.Application.Features.DeleteProduct;
using Krepim.Catalog.Application.Features.PublishProduct;
using Krepim.Catalog.Application.Features.SearchProduct;
using Krepim.Catalog.Application.Features.UpdateProduct;
using Krepim.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Krepim.Catalog.Api.Endpoints.Products
{
    internal static class ManagerEndpoints
    {
        public static void MapManagerEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/manager").RequireAuthorization(policy => policy.RequireRole("Manager"));

            group.MapGet("/search", async ([FromQuery] string term, [FromQuery] int page, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new SearchProductsQuery(term, OnlyActive: false, page, 20), ct);
                return result.Match(products => Microsoft.AspNetCore.Http.Results.Ok(products));
            });

            group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateProductCommand command, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(command with { Id = id }, ct);
                return result.Match(() => Microsoft.AspNetCore.Http.Results.NoContent());
            });

            group.MapPost("/{id:guid}/publish", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new PublishProductCommand(id), ct);
                return result.Match(() => Microsoft.AspNetCore.Http.Results.NoContent());
            });

            group.MapPost("/{id:guid}/deactivate", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new DeactivateProductCommand(id), ct);
                return result.Match(() => Microsoft.AspNetCore.Http.Results.NoContent());
            });

            group.MapDelete("/{id:guid}", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new DeleteProductCommand(id), ct);
                return result.Match(() => Microsoft.AspNetCore.Http.Results.NoContent());
            });
        }
    }
}