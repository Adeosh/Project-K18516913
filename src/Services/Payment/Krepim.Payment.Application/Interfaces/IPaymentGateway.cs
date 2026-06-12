using Krepim.Payment.Application.Models.Exchange;

namespace Krepim.Payment.Application.Interfaces
{
    public interface IPaymentGateway
    {
        Task<PaymentGatewayResponse> InitializePaymentAsync(Guid orderId, decimal amount, CancellationToken ct);
    }
}
