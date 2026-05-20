using Krepim.Catalog.Application.Features.CreateProduct;
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

                await sender.Send(command, ct))

            .WithName("CreateProduct")
            .WithSummary("Создать новый товар (Write Model)")
            .RequireAuthorization(policy => policy.RequireRole("Manager"))
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        }
    }
}
