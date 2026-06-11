using Krepim.Catalog.Application.Features.GetCategories;
using Krepim.Catalog.Application.Models;
using Krepim.SharedKernel.Results.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Krepim.Catalog.Api.Endpoints.Categories
{
    internal static class CategoryEndpoints
    {
        public static void MapCategoryEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/categories")
                .WithTags("Categories")
                .AddEndpointFilter<ResultEndpointFilter>();

            group.MapPost("/", async ([FromBody] Application.Features.CreateCategory.CreateCategoryCommand command, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            })
            .WithName("CreateCategory")
            .RequireAuthorization(policy => policy.RequireRole("Manager"));

            group.MapGet("/", async ([FromServices] ISender sender, CancellationToken ct) =>
            {
                return await sender.Send(new GetCategoriesQuery(), ct);
            })
            .WithName("GetCategories")
            .WithSummary("Получить список активных категорий")
            .Produces<IReadOnlyList<CategoryModel>>(StatusCodes.Status200OK);

            group.MapDelete("/{id:guid}", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Krepim.Catalog.Application.Features.DeleteCategory.DeleteCategoryCommand(id), ct);
                return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
            })
            .WithName("DeleteCategory")
            .WithSummary("Мягкое удаление категории")
            .RequireAuthorization(policy => policy.RequireRole("Manager"));
        }
    }
}
