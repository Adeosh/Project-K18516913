using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Inventory.Application.Features.CreditStock
{
    public sealed record CreditStockCommand(Guid ProductId, int Quantity) : IRequest<Result>;
}
