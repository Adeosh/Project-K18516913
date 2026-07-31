using Krepim.Inventory.Domain.Entities;
using Krepim.Inventory.Domain.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Inventory.Application.Features.CreditStock
{
    internal sealed class CreditStockCommandHandler(
        IInventoryRepository repository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreditStockCommand, Result>
    {
        public async Task<Result> Handle(CreditStockCommand request, CancellationToken ct)
        {
            StockItem? stockItem = await repository.GetByProductIdAsync(request.ProductId, ct);

            if (stockItem is null)
            {
                Result<StockItem> createResult = StockItem.Create(request.ProductId, request.Quantity);

                if (createResult.IsFailure)
                    return Result.Failure(createResult.Error);

                await repository.AddAsync(createResult.Value, ct);
            }
            else
            {
                Result creditResult = stockItem.CreditStock(request.Quantity);

                if (creditResult.IsFailure)
                    return creditResult;
            }

            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
