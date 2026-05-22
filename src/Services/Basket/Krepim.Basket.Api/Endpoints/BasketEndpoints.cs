using Krepim.Basket.Application.Features.AddItem;
using Krepim.Basket.Application.Features.Checkout;
using Krepim.Basket.Application.Features.ClearBasket;
using Krepim.Basket.Application.Features.GetBasket;
using Krepim.Basket.Application.Features.RemoveItem;
using Krepim.Basket.Application.Models;
using Krepim.SharedKernel.Extensions;
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
                .RequireAuthorization();

            group.MapGet("/", async (ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetBasketQuery(GetUserId(user)), ct);
                return result.Match(basket => Microsoft.AspNetCore.Http.Results.Ok(basket));
            })
            .WithName("GetBasket");

            group.MapPost("/items", async (
                ClaimsPrincipal user,
                [FromBody] Application.Models.AddItemRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new AddItemToBasketCommand(
                    GetUserId(user),
                    request.ProductId,
                    request.ProductName,
                    request.Sku,
                    request.UnitPrice,
                    request.Quantity);

                var result = await sender.Send(command, ct);
                return result.Match(basket => Microsoft.AspNetCore.Http.Results.Ok(basket));
            })
            .WithName("AddItemToBasket");

            group.MapDelete("/items/{productId:guid}", async (
                ClaimsPrincipal user,
                Guid productId,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new RemoveItemFromBasketCommand(GetUserId(user), productId), ct);
                return result.Match(basket => Microsoft.AspNetCore.Http.Results.Ok(basket));
            })
            .WithName("RemoveItemFromBasket");

            group.MapDelete("/", async (ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new ClearBasketCommand(GetUserId(user)), ct);
                return result.Match(() => Microsoft.AspNetCore.Http.Results.NoContent());
            })
            .WithName("ClearBasket");

            group.MapPost("/checkout", async (
                ClaimsPrincipal user,
                [FromBody] CheckoutRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new CheckoutBasketCommand(
                    GetUserId(user),
                    request.City,
                    request.Street,
                    request.ZipCode);

                var result = await sender.Send(command, ct);

                return result.Match(() => Microsoft.AspNetCore.Http.Results.Accepted());
            })
            .WithName("CheckoutBasket");
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
