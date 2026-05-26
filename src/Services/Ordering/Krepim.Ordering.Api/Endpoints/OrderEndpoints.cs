using Krepim.Ordering.Application.Features.GetMyOrders;
using Krepim.SharedKernel.Results.Filters;
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
                .RequireAuthorization()
                .AddEndpointFilter<ResultEndpointFilter>(); ;

            group.MapGet("/", async (ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken ct) =>
            {
                return await sender.Send(new GetMyOrdersQuery(GetUserId(user)), ct);
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
