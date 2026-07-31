using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.EventBus.Events.Catalog;
using Krepim.EventBus.Events.Catalog.Models;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
using Krepim.SharedKernel.ValueObjects;
using MassTransit;
using MediatR;

namespace Krepim.Catalog.Application.Features.UpdateProduct
{
    internal sealed class UpdateProductCommandHandler(
        IProductWriteRepository repository, 
        IPublishEndpoint publishEndpoint, 
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductCommand, Result>
    {
        public async Task<Result> Handle(UpdateProductCommand request, CancellationToken ct)
        {
            Product? product = await repository.GetByIdAsync(request.Id, ct);
            if (product is null || product.IsDeleted) 
                return Result.Failure(new Error("Product.NotFound", "Not found", ErrorType.NotFound));

            product.UpdateDetails(
                request.Name,
                request.Description,
                request.CategoryId,
                request.Standard,
                request.SalesUnit,
                request.SalesStep);

            product.SetImages(request.ImageUrls ?? Array.Empty<string>());

            if (request.Attributes != null)
                product.SetAttributes(request.Attributes);

            if (request.PriceTiers != null)
            {
                IEnumerable<PriceTier> domainTiers = request.PriceTiers.Select(pt =>
                    new PriceTier(pt.MinQuantity, new Money(pt.Amount, pt.Currency)));

                product.UpdatePriceTiers(domainTiers);
            }

            ProductUpdatedIntegrationEvent integrationEvent = new ProductUpdatedIntegrationEvent(
                product.Id,
                product.Name,
                product.Description,
                product.Price.Amount,
                product.Price.Currency,
                product.CategoryId,
                request.ImageUrls ?? Array.Empty<string>(),
                product.Standard,
                (int)product.SalesUnit,
                product.SalesStep,
                product.Attributes.ToDictionary(k => k.Key, v => v.Value),
                product.PriceTiers.Select(pt => new PriceTierModel(pt.MinQuantity, pt.Price.Amount, pt.Price.Currency)).ToList()
            );

            await publishEndpoint.Publish(integrationEvent, ct);

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
    }
}
