using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.EventBus.Events.Catalog;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
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
            var result = Product.Create(request.Name, request.Description, request.Sku, request.Price, request.CategoryId);
            if (result.IsFailure) 
                return Result<Guid>.Failure(result.Error);

            var product = result.Value;

            await productRepository.AddAsync(product, cancellationToken);

            var integrationEvent = new ProductCreatedIntegrationEvent(
                product.Id,
                product.Name,
                product.Description,
                product.Sku.Value,
                product.Price.Amount,
                product.Price.Currency,
                product.CategoryId);

            await publishEndpoint.Publish(integrationEvent, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return product.Id;
        }
    }
}
