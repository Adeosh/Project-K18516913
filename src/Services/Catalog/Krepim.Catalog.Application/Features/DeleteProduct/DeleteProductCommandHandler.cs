using Krepim.Catalog.Application.Interfaces;
using Krepim.EventBus.Events.Catalog;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
using MassTransit;
using MediatR;

namespace Krepim.Catalog.Application.Features.DeleteProduct
{
    internal sealed class DeleteProductCommandHandler(
        IProductWriteRepository repository, 
        IPublishEndpoint publishEndpoint, 
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductCommand, Result>
    {
        public async Task<Result> Handle(DeleteProductCommand request, CancellationToken ct)
        {
            var product = await repository.GetByIdAsync(request.Id, ct);
            if (product is null || product.IsDeleted)
                return Result.Success();

            product.Delete();

            await publishEndpoint.Publish(new ProductDeletedIntegrationEvent(product.Id), ct);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
