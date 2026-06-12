using Krepim.Identity.Application.Models.DTOs;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Identity.Application.Features.Profile.UpdateUserProfile
{
    public record UpdateUserProfileCommand(
        Guid UserId,
        string Email,
        string? PhoneNumber,
        AddressDto? DefaultAddress) : IRequest<Result>;
}
