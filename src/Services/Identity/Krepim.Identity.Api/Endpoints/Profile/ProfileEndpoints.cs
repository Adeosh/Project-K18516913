using Krepim.Identity.Application.Features.Profile.ChangePassword;
using Krepim.Identity.Application.Features.Profile.GetUserProfile;
using Krepim.Identity.Application.Features.Profile.UpdateUserProfile;
using Krepim.Identity.Application.Models.Exchange;
using Krepim.SharedKernel.Results;
using MediatR;
using System.Security.Claims;

namespace Krepim.Identity.Api.Endpoints.Profile
{
    public static class ProfileEndpoints
    {
        public static void MapProfileEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/profile").RequireAuthorization();

            group.MapGet("/", async (ClaimsPrincipal user, ISender sender) =>
            {
                var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

                if (!Guid.TryParse(userIdStr, out var userId))
                    return Results.Unauthorized();

                var query = new GetUserProfileQuery(userId);
                var result = await sender.Send(query);

                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

            group.MapPut("/", async (UpdateProfileRequest request, ClaimsPrincipal user, ISender sender) =>
            {
                var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

                if (!Guid.TryParse(userIdStr, out var userId))
                    return Results.Unauthorized();

                var command = new UpdateUserProfileCommand(userId, request.Email, request.PhoneNumber, request.DefaultAddress);
                var result = await sender.Send(command);

                return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
            });

            group.MapPost("/change-password", async (ChangePasswordRequest request, ClaimsPrincipal user, ISender sender) =>
            {
                var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
                if (!Guid.TryParse(userIdStr, out var userId)) return Results.Unauthorized();

                if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                    return Results.BadRequest(new Error("Password.TooShort", "Новый пароль должен быть не менее 6 символов", ErrorType.Validation));

                var command = new ChangePasswordCommand(userId, request.OldPassword, request.NewPassword);
                var result = await sender.Send(command);

                return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
            });
        }
    }
}
