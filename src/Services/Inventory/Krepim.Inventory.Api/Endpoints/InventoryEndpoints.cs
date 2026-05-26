using Krepim.Inventory.Application.Features.GetStock;
using Krepim.SharedKernel.Results.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Krepim.Inventory.Api.Endpoints
{
    internal static class InventoryEndpoints
    {
        public static void MapInventoryEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/api/inventory")
                .WithTags("Inventory")
                .AddEndpointFilter<ResultEndpointFilter>(); ;

            group.MapGet("/{productId:guid}", async (
                Guid productId,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                return await sender.Send(new GetStockQuery(productId), ct);
            })
            .WithName("GetStock");
        }
    }
}
