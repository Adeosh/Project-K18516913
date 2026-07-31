using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.EventBus.Events.Catalog;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
using MassTransit;
using MediatR;

namespace Krepim.Catalog.Application.Features.PublishProduct
{
    internal sealed class PublishProductCommandHandler(
        IProductWriteRepository repository, 
        IPublishEndpoint publishEndpoint, 
        IUnitOfWork unitOfWork) : IRequestHandler<PublishProductCommand, Result>
    {
        public async Task<Result> Handle(PublishProductCommand request, CancellationToken ct)
        {
            Product? product = await repository.GetByIdAsync(request.Id, ct);
            if (product is null || product.IsDeleted) 
                return Result.Failure(new Error("Product.NotFound", "Not found", ErrorType.NotFound));

            product.Publish();

            await publishEndpoint.Publish(new ProductStatusChangedIntegrationEvent(product.Id, true), ct);
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
    }
}
