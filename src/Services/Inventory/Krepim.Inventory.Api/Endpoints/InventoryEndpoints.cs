using Krepim.Inventory.Application.Features.GetStock;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Krepim.Inventory.Api.Endpoints
{
    internal static class InventoryEndpoints
    {
        public static void MapInventoryEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/api/inventory")
                .WithTags("Inventory");

            group.MapGet("/{productId:guid}", async (
                Guid productId,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetStockQuery(productId), ct);
                return result.Match(stock => Microsoft.AspNetCore.Http.Results.Ok(stock));
            })
            .WithName("GetStock");
        }
    }
}
