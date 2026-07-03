using Krepim.SharedKernel.Enums;
using System.Text.Json.Serialization;

namespace Krepim.Payment.Application.Models.Exchange
{
    public record MockWebhookRequest(
        string TransactionId,
        [property: JsonConverter(typeof(JsonStringEnumConverter))] PaymentStatus Status, 
        string? ErrorMessage = null);
}
