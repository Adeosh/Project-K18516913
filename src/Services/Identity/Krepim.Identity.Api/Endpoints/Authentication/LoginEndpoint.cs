using Krepim.Identity.Application.Features.Authentication;
using Krepim.SharedKernel.Results.Filters;
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
                return await sender.Send(command, ct);
            })
            .AddEndpointFilter<ResultEndpointFilter>()
            .WithName("LoginUser")
            .WithSummary("Аутентификация пользователя")
            .Produces<string>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        }
    }
}