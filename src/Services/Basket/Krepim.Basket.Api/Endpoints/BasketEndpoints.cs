using Krepim.Basket.Application.Features.AddItem;
using Krepim.Basket.Application.Features.Checkout;
using Krepim.Basket.Application.Features.ClearBasket;
using Krepim.Basket.Application.Features.GetBasket;
using Krepim.Basket.Application.Features.RemoveItem;
using Krepim.Basket.Application.Features.UpdateItemQuantity;
using Krepim.Basket.Application.Models.Exchange;
using Krepim.SharedKernel.Extensions;
using Krepim.SharedKernel.Results.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Krepim.Basket.Api.Endpoints
{
    internal static class BasketEndpoints
    {
        public static void MapBasketEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/api/basket")
                .WithTags("Basket")
                .RequireAuthorization()
                .AddEndpointFilter<ResultEndpointFilter>();

            group.MapGet("/", async (ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken ct) =>
                await sender.Send(new GetBasketQuery(GetUserId(user)), ct))
            .WithName("GetBasket");

            group.MapPost("/items", async (
                ClaimsPrincipal user,
                [FromBody] AddItemRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                return await sender.Send(new AddItemToBasketCommand(
                    GetUserId(user), request.ProductId, request.ProductName, request.Sku, request.UnitPrice, request.Quantity), ct);
            })
            .WithName("AddItemToBasket");

            group.MapPut("/items/{productId:guid}", async (
                ClaimsPrincipal user,
                Guid productId,
                [FromBody] UpdateQuantityRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                return await sender.Send(new UpdateItemQuantityCommand(GetUserId(user), productId, request.Quantity, request.Price), ct);
            })
            .WithName("UpdateItemQuantity");

            group.MapDelete("/items/{productId:guid}", async (ClaimsPrincipal user, Guid productId, [FromServices] ISender sender, CancellationToken ct) =>
                await sender.Send(new RemoveItemFromBasketCommand(GetUserId(user), productId), ct))
            .WithName("RemoveItemFromBasket");

            group.MapDelete("/", async (ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken ct) =>
                await sender.Send(new ClearBasketCommand(GetUserId(user)), ct))
            .WithName("ClearBasket");

            group.MapPost("/checkout", async (
            ClaimsPrincipal user,
            [FromBody] CheckoutRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
            {
                var generatedOrderId = Guid.NewGuid();

                var result = await sender.Send(new CheckoutBasketCommand(
                    GetUserId(user), generatedOrderId, request.FullAddress, request.Latitude, request.Longitude, request.Flat), ct);

                return result.Match(() => Microsoft.AspNetCore.Http.Results.Ok(new { orderId = generatedOrderId }));
            })
            .WithName("CheckoutBasket")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
