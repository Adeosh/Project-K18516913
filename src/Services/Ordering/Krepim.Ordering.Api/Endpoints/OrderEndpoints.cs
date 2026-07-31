using Krepim.Ordering.Application.Features.GetAllOrders;
using Krepim.Ordering.Application.Features.GetMyOrders;
using Krepim.Ordering.Application.Features.GetOrderById;
using Krepim.Ordering.Application.Models.DTOs;
using Krepim.SharedKernel.Results;
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
            var group = builder.MapGroup("")
                .WithTags("Orders")
                .RequireAuthorization()
                .AddEndpointFilter<ResultEndpointFilter>();

            group.MapGet("/", async (ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken ct) =>
            {
                return await sender.Send(new GetMyOrdersQuery(GetUserId(user)), ct);
            })
            .WithName("GetMyOrders");

            group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
            {
                Result<OrderDto> result = await sender.Send(new GetOrderByIdQuery(id));
                return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
            }).RequireAuthorization();

            group.MapGet("/all", async ([FromServices] ISender sender, CancellationToken ct) =>
            {
                return await sender.Send(new GetAllOrdersQuery(), ct);
            })
            .WithName("GetAllOrders");
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            string? userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
