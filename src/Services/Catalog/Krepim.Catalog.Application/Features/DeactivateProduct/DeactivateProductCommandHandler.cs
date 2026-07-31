using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.EventBus.Events.Catalog;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
using MassTransit;
using MediatR;

namespace Krepim.Catalog.Application.Features.DeactivateProduct
{
    internal sealed class DeactivateProductCommandHandler(
        IProductWriteRepository repository, 
        IPublishEndpoint publishEndpoint, 
        IUnitOfWork unitOfWork) : IRequestHandler<DeactivateProductCommand, Result>
    {
        public async Task<Result> Handle(DeactivateProductCommand request, CancellationToken ct)
        {
            Product? product = await repository.GetByIdAsync(request.Id, ct);
            if (product is null || product.IsDeleted)
                return Result.Failure(new Error("Product.NotFound", "Not found", ErrorType.NotFound));

            product.Deactivate();

            await publishEndpoint.Publish(new ProductStatusChangedIntegrationEvent(product.Id, false), ct);
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
    }
}
