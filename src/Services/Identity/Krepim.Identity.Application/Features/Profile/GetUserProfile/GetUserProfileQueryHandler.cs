using Krepim.Identity.Application.Models.DTOs;
using Krepim.Identity.Application.Models.Exchange;
using Krepim.Identity.Domain.Interfaces;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Identity.Application.Features.Profile.GetUserProfile
{
    internal sealed class GetUserProfileQueryHandler(IUserRepository userRepository)
        : IRequestHandler<GetUserProfileQuery, Result<UserProfileResponse>>
    {
        public async Task<Result<UserProfileResponse>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

            if (user is null)
                return Result<UserProfileResponse>.Failure(new Error("User.NotFound", "User not found", ErrorType.NotFound));

            var addressDto = user.DefaultAddress is not null
                ? new AddressDto(user.DefaultAddress.FullAddress, user.DefaultAddress.Latitude, user.DefaultAddress.Longitude, user.DefaultAddress.Flat)
                : null;

            var response = new UserProfileResponse(
                user.Email,
                user.Role.ToString(),
                user.PhoneNumber,
                addressDto);

            return response;
        }
    }
}
