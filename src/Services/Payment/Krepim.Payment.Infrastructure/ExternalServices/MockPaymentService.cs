using Krepim.Payment.Application.Interfaces;
using Krepim.Payment.Application.Models.Exchange;

namespace Krepim.Payment.Infrastructure.ExternalServices
{
    public sealed class MockPaymentService : IPaymentGateway
    {
        public Task<PaymentGatewayResponse> InitializePaymentAsync(Guid orderId, decimal amount, CancellationToken ct)
        {
            var fakeExternalId = $"mock_tx_{Guid.NewGuid():N}";

            var paymentUrl = $"http://localhost:5173/mock-pay?tx={fakeExternalId}&orderId={orderId}&amount={amount}";

            var response = new PaymentGatewayResponse(fakeExternalId, paymentUrl);

            return Task.FromResult(response);
        }
    }
}
