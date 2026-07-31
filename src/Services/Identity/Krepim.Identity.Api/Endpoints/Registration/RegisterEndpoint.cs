using Krepim.Identity.Application.Features.Registration;
using Krepim.SharedKernel.Extensions;
using Krepim.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Krepim.Identity.Api.Endpoints.Registration
{
    internal static class RegisterEndpoint
    {
        public static void MapRegisterEndpoint(this IEndpointRouteBuilder builder)
        {
            builder.MapPost("/register", async (
                [FromBody] RegisterCommand command,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                Result<Guid> result = await sender.Send(command, ct);

                return result.Match(userId => Microsoft.AspNetCore.Http.Results.Created($"/api/identity/{userId}", userId));
            })
            .WithName("RegisterUser")
            .WithSummary("Регистрация нового пользователя")
            .WithDescription("Создает учетную запись и возвращает ID пользователя.")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);
        }
    }
}