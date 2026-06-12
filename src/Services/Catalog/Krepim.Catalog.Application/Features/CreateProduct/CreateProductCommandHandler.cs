using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.EventBus.Events.Catalog;
using Krepim.EventBus.Events.Catalog.Models;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
using Krepim.SharedKernel.ValueObjects;
using MassTransit;
using MediatR;

namespace Krepim.Catalog.Application.Features.CreateProduct
{
    internal sealed class CreateProductCommandHandler(
        IProductWriteRepository productRepository,
        IPublishEndpoint publishEndpoint,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var result = Product.Create(
                request.Name,
                request.Description,
                request.Sku,
                request.Price,
                request.CategoryId,
                request.Standard,
                request.SalesUnit,
                request.SalesStep);

            if (result.IsFailure)
                return Result<Guid>.Failure(result.Error);

            var product = result.Value;

            if (request.ImageUrls != null && request.ImageUrls.Any())
                product.SetImages(request.ImageUrls);

            if (request.Attributes != null && request.Attributes.Any())
                product.SetAttributes(request.Attributes);

            if (request.PriceTiers != null && request.PriceTiers.Any())
            {
                var domainTiers = request.PriceTiers.Select(pt =>
                    new PriceTier(pt.MinQuantity, new Money(pt.Amount, pt.Currency)));

                product.UpdatePriceTiers(domainTiers);
            }

            await productRepository.AddAsync(product, cancellationToken);

            var integrationEvent = new ProductCreatedIntegrationEvent(
                product.Id,
                product.Name,
                product.Description,
                product.Sku.Value,
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

            await publishEndpoint.Publish(integrationEvent, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return product.Id;
        }
    }
}
