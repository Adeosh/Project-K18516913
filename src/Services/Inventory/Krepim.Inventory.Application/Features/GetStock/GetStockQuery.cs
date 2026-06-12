using Krepim.Inventory.Application.Models.DTOs;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Inventory.Application.Features.GetStock
{
    public sealed record GetStockQuery(Guid ProductId) : IRequest<Result<StockDto>>;
}
