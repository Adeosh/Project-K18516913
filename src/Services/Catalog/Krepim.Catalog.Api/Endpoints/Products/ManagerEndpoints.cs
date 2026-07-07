using Krepim.Catalog.Application.Features.DeactivateProduct;
using Krepim.Catalog.Application.Features.DeleteProduct;
using Krepim.Catalog.Application.Features.PublishProduct;
using Krepim.Catalog.Application.Features.SearchProduct;
using Krepim.Catalog.Application.Features.UpdateProduct;
using Krepim.SharedKernel.Results.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Krepim.Catalog.Api.Endpoints.Products
{
    internal static class ManagerEndpoints
    {
        public static void MapManagerEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/manager")
                .RequireAuthorization(policy => policy.RequireRole("Manager"))
                .AddEndpointFilter<ResultEndpointFilter>();

            group.MapGet("/search", async ([FromQuery] string? term, [FromQuery] int? page, [FromQuery] Guid? categoryId, [FromServices] ISender sender, CancellationToken ct) =>
                await sender.Send(new SearchProductsQuery(
                    SearchTerm: term ?? string.Empty,
                    OnlyActive: false,
                    Page: page ?? 1,
                    PageSize: 20,
                    CategoryId: categoryId), ct));

            group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateProductCommand command, [FromServices] ISender sender, CancellationToken ct) =>
                await sender.Send(command with { Id = id }, ct));

            group.MapPost("/{id:guid}/publish", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
                await sender.Send(new PublishProductCommand(id), ct));

            group.MapPost("/{id:guid}/deactivate", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
                await sender.Send(new DeactivateProductCommand(id), ct));

            group.MapDelete("/{id:guid}", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
                await sender.Send(new DeleteProductCommand(id), ct));
        }
    }
}