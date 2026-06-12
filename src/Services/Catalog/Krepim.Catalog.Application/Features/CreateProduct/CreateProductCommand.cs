using Krepim.Catalog.Application.Models.DTOs;
using Krepim.Catalog.Domain.Enums;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.CreateProduct
{
    public sealed record CreateProductCommand(
        string Name,
        string Description,
        string Sku,
        decimal Price,
        Guid CategoryId,
        string[]? ImageUrls,
        string? Standard,
        SalesUnit SalesUnit,
        decimal SalesStep,
        Dictionary<string, string>? Attributes,
        List<PriceTierDto>? PriceTiers) : IRequest<Result<Guid>>;
}
