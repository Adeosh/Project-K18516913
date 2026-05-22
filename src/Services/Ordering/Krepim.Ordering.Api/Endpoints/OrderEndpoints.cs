using Krepim.Ordering.Application.Features.GetMyOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Krepim.Ordering.Api.Endpoints
{
    internal static class OrderEndpoints
    {
        public static void MapOrderEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/api/orders")
                .WithTags("Orders")
                .RequireAuthorization();

            group.MapGet("/", async (ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetMyOrdersQuery(GetUserId(user)), ct);
                return result.Match(orders => Microsoft.AspNetCore.Http.Results.Ok(orders));
            })
            .WithName("GetMyOrders");
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
