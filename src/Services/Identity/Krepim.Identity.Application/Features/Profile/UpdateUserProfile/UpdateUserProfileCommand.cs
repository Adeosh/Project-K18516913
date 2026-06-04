using Krepim.Identity.Application.Models;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Identity.Application.Features.Profile.UpdateUserProfile
{
    public record UpdateUserProfileCommand(
        Guid UserId,
        string Email,
        string? PhoneNumber,
        AddressModel? DefaultAddress) : IRequest<Result>;
}
