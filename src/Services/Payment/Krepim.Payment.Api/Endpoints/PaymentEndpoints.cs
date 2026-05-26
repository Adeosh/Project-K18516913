using Krepim.Payment.Application.Features.GetPaymentUrl;
using Krepim.SharedKernel.Results.Filters;
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
                .RequireAuthorization()
                .AddEndpointFilter<ResultEndpointFilter>(); ;

            group.MapGet("/{orderId:guid}/url", async (
                Guid orderId,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                return await sender.Send(new GetPaymentUrlQuery(orderId), ct);
            })
            .WithName("GetPaymentUrl");
        }
    }
}
