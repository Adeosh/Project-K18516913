using Krepim.Identity.Application.Models.Exchange;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Identity.Application.Features.Profile.GetUserProfile
{
    public record GetUserProfileQuery(Guid UserId) : IRequest<Result<UserProfileResponse>>;
}
