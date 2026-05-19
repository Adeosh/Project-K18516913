using Krepim.Identity.Application.Features.Registration;
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

                await sender.Send(command, ct))

            .WithName("RegisterUser")
            .WithSummary("Регистрация нового пользователя (Client или Manager)")
            .WithDescription("Создает учетную запись и возвращает ID пользователя.")
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);
        }
    }
}
