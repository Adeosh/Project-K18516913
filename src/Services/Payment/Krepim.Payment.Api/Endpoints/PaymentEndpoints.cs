using Krepim.Payment.Application.Features.GetPaymentUrl;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Krepim.Payment.Api.Endpoints
{
    internal static class PaymentEndpoints
    {
        public static void MapPaymentEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/api/payments")
                .WithTags("Payments")
                .RequireAuthorization();

            group.MapGet("/{orderId:guid}/url", async (
                Guid orderId,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetPaymentUrlQuery(orderId), ct);

                return result.Match(url => Microsoft.AspNetCore.Http.Results.Ok(new { Url = url }));
            })
            .WithName("GetPaymentUrl");
        }
    }
}
