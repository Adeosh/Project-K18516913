using Krepim.Identity.Application.Models.DTOs;

namespace Krepim.Identity.Application.Models.Exchange
{
    public record UserProfileResponse(
        string Email,
        string Role,
        string? PhoneNumber,
        AddressDto? DefaultAddress);
}
