using Krepim.Payment.Domain.Interfaces;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Payment.Application.Features.GetPaymentUrl
{
    internal sealed class GetPaymentUrlQueryHandler(IPaymentRepository repository)
        : IRequestHandler<GetPaymentUrlQuery, Result<string>>
    {
        public async Task<Result<string>> Handle(GetPaymentUrlQuery request, CancellationToken ct)
        {
            var transaction = await repository.GetByOrderIdAsync(request.OrderId, ct);

            if (transaction is null || string.IsNullOrWhiteSpace(transaction.PaymentUrl))
            {
                return Result<string>.Failure(new Error(
                    "Payment.UrlNotFound",
                    "Ссылка на оплату для данного заказа еще не сформирована или заказ не найден.",
                    ErrorType.NotFound));
            }

            return transaction.PaymentUrl;
        }
    }
}
