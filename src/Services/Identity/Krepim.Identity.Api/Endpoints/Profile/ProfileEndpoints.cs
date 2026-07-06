using Krepim.Identity.Application.Features.Profile.ChangePassword;
using Krepim.Identity.Application.Features.Profile.GetUserProfile;
using Krepim.Identity.Application.Features.Profile.UpdateUserProfile;
using Krepim.Identity.Application.Models.Exchange;
using Krepim.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Krepim.Identity.Api.Endpoints.Profile
{
    public static class ProfileEndpoints
    {
        public static void MapProfileEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/profile").RequireAuthorization();

            group.MapGet("", async (
                ClaimsPrincipal user,
                [FromServices] ISender sender) =>
            {
                var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

                if (!Guid.TryParse(userIdStr, out var userId))
                    return Result<UserProfileResponse>.Failure(new Error("Auth.Unauthorized", "Пользователь не авторизован", ErrorType.Unauthorized));

                return await sender.Send(new GetUserProfileQuery(userId));
            });

            group.MapPut("", async (
                [FromBody] UpdateProfileRequest request,
                ClaimsPrincipal user,
                [FromServices] ISender sender) =>
            {
                var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

                if (!Guid.TryParse(userIdStr, out var userId))
                    return Result.Failure(new Error("Auth.Unauthorized", "Пользователь не авторизован", ErrorType.Unauthorized));

                return await sender.Send(new UpdateUserProfileCommand(userId, request.Email, request.PhoneNumber, request.DefaultAddress));
            });

            group.MapPost("/change-password", async (
                [FromBody] ChangePasswordRequest request,
                ClaimsPrincipal user,
                [FromServices] ISender sender) =>
            {
                var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

                if (!Guid.TryParse(userIdStr, out var userId))
                    return Result.Failure(new Error("Auth.Unauthorized", "Пользователь не авторизован", ErrorType.Unauthorized));

                if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                    return Result.Failure(new Error("Password.TooShort", "Новый пароль должен быть не менее 6 символов", ErrorType.Validation));

                return await sender.Send(new ChangePasswordCommand(userId, request.OldPassword, request.NewPassword));
            });
        }
    }
}