namespace Krepim.Payment.Application.Models
{
    public record PaymentGatewayResponse(string ExternalId, string PaymentUrl);
}
