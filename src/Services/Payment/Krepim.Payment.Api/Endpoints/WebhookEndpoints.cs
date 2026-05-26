using Krepim.Payment.Application.Features.CompletePayment;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Krepim.Payment.Api.Endpoints
{
    internal static class WebhookEndpoints
    {
        public static void MapWebhookEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/api/payments/webhook")
                .WithTags("Webhooks");

            group.MapPost("/", async (
                HttpRequest request,
                [FromServices] ISender sender,
                [FromServices] IConfiguration config,
                [FromServices] ILogger<Program> logger,
                CancellationToken ct) =>
            {
                var json = await new StreamReader(request.Body).ReadToEndAsync(ct);
                var signature = request.Headers["Stripe-Signature"].ToString();
                var webhookSecret = config["PaymentSettings:WebhookSecret"];

                try
                {
                    var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);

                    if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
                    {
                        var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                        var externalId = session!.PaymentIntentId;
                        var result = await sender.Send(new CompletePaymentCommand(externalId), ct);

                        if (result.IsFailure)
                            logger.LogWarning("Логическая ошибка при обработке вебхука: {Error}", result.Error.Description);
                    }

                    return Microsoft.AspNetCore.Http.Results.Ok();
                }
                catch (StripeException e)
                {
                    logger.LogError(e, "Неверная подпись вебхука!");
                    return Microsoft.AspNetCore.Http.Results.BadRequest();
                }
            })
            .WithName("StripeWebhook")
            .WithSummary("Прием вебхуков от платежной системы Stripe");
        }
    }
}
