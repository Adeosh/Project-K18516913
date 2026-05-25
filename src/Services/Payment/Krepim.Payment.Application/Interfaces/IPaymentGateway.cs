using Krepim.Payment.Application.Models;

namespace Krepim.Payment.Application.Interfaces
{
    public interface IPaymentGateway
    {
        Task<PaymentGatewayResponse> InitializePaymentAsync(Guid orderId, decimal amount, CancellationToken ct);
    }
}
