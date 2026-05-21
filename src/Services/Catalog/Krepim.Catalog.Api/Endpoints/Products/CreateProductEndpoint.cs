using Krepim.Catalog.Application.Features.CreateProduct;
using Krepim.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Krepim.Catalog.Api.Endpoints.Products
{
    internal static class CreateProductEndpoint
    {
        public static void MapCreateProductEndpoint(this IEndpointRouteBuilder builder)
        {
            builder.MapPost("/", async (
                [FromBody] CreateProductCommand command,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(command, ct);

                return result.Match(productId =>
                    Microsoft.AspNetCore.Http.Results.Created($"/api/products/{productId}", productId));
            })
            .WithName("CreateProduct")
            .WithSummary("Создать новый товар (Write Model)")
            .RequireAuthorization(policy => policy.RequireRole("Manager"))
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        }
    }
}
