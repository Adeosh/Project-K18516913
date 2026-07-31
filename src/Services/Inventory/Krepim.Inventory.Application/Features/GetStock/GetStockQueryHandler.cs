using Krepim.Inventory.Application.Models.DTOs;
using Krepim.Inventory.Domain.Entities;
using Krepim.Inventory.Domain.Interfaces;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Inventory.Application.Features.GetStock
{
    internal sealed class GetStockQueryHandler(IInventoryRepository repository)
        : IRequestHandler<GetStockQuery, Result<StockDto>>
    {
        public async Task<Result<StockDto>> Handle(GetStockQuery request, CancellationToken ct)
        {
            StockItem? stockItem = await repository.GetByProductIdAsync(request.ProductId, ct);

            if (stockItem is null)
            {
                return Result<StockDto>.Failure(new Error(
                    "Stock.NotFound",
                    $"Товар с ID {request.ProductId} не найден на складе.",
                    ErrorType.NotFound));
            }

            return Result<StockDto>.Success(new StockDto(stockItem.ProductId, stockItem.AvailableQuantity));
        }
    }
}
