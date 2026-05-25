using Krepim.Inventory.Application.Interfaces;
using Krepim.Inventory.Application.Models;
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
                return new StockModel(request.ProductId, 0);

            return new StockModel(stockItem.ProductId, stockItem.AvailableQuantity);
        }
    }
}
