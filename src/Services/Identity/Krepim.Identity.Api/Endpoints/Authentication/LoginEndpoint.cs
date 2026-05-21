using Krepim.Identity.Application.Features.Authentication;
using Krepim.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Krepim.Identity.Api.Endpoints.Authentication
{
    internal static class LoginEndpoint
    {
        public static void MapLoginEndpoint(this IEndpointRouteBuilder builder)
        {
            builder.MapPost("/login", async (
                [FromBody] LoginCommand command,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(command, ct);

                return result.Match(token => Microsoft.AspNetCore.Http.Results.Ok(token));
            })
            .WithName("LoginUser")
            .WithSummary("Аутентификация пользователя")
            .Produces<string>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        }
    }
}