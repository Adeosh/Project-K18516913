using Krepim.Catalog.Application.Models.DTOs;
using Krepim.Catalog.Domain.Enums;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Catalog.Application.Features.UpdateProduct
{
    public sealed record UpdateProductCommand(
        Guid Id,
        string Name,
        string Description,
        Guid CategoryId,
        string[]? ImageUrls,
        string? Standard,
        SalesUnit SalesUnit,
        decimal SalesStep,
        Dictionary<string, string>? Attributes,
        List<PriceTierDto>? PriceTiers) : IRequest<Result>;
}
