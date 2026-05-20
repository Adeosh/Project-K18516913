using Krepim.Catalog.Application.Interfaces;
using Krepim.EventBus.Events.Catalog;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
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
            var product = await repository.GetByIdAsync(request.Id, ct);
            if (product is null || product.IsDeleted) 
                return Result.Failure(new Error("Product.NotFound", "Not found", ErrorType.NotFound));

            product.UpdateDetails(request.Name, request.Description, request.CategoryId);

            await publishEndpoint.Publish(new ProductUpdatedIntegrationEvent(
                product.Id, product.Name, product.Description, product.Price.Amount, product.Price.Currency, product.CategoryId), ct);

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
    }
}
