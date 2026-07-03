using Krepim.Payment.Application.Features.CompletePayment;
using Krepim.Payment.Application.Models.Exchange;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Krepim.Payment.Api.Endpoints
{
    internal static class WebhookEndpoints
    {
        public static void MapWebhookEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/api/payments/webhook")
                .WithTags("Webhooks");

            group.MapPost("/mock", async (
                [FromBody] MockWebhookRequest request,
                [FromServices] ISender sender,
                [FromServices] ILogger<Program> logger,
                CancellationToken ct) =>
            {
                logger.LogInformation("Получен тестовый вебхук для транзакции {TxId} со статусом {Status}",
                    request.TransactionId, request.Status);

                var result = await sender.Send(
                    new ProcessPaymentWebhookCommand(
                        request.TransactionId,
                        request.Status,
                        request.ErrorMessage),
                    ct);

                if (result.IsFailure)
                {
                    logger.LogWarning("Ошибка при обработке тестового вебхука: {Error}", result.Error.Description);
                    return Results.BadRequest(result.Error);
                }

                return Microsoft.AspNetCore.Http.Results.Ok();
            })
            .WithName("MockWebhook")
            .WithSummary("Прием тестовых вебхуков для симуляции оплаты");
        }
    }
}
