using Krepim.Inventory.Application.Models;
using Krepim.Inventory.Domain.Interfaces;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Inventory.Application.Features.GetStock
{
    internal sealed class GetStockQueryHandler(IInventoryRepository repository)
        : IRequestHandler<GetStockQuery, Result<StockModel>>
    {
        public async Task<Result<StockModel>> Handle(GetStockQuery request, CancellationToken ct)
        {
            var stockItem = await repository.GetByProductIdAsync(request.ProductId, ct);

            if (stockItem is null)
            {
                return Result<StockModel>.Failure(new Error(
                    "Stock.NotFound",
                    $"Товар с ID {request.ProductId} не найден на складе.",
                    ErrorType.NotFound));
            }

            return Result<StockModel>.Success(new StockModel(stockItem.ProductId, stockItem.AvailableQuantity));
        }
    }
}
