using Krepim.Mobile.Features.Profile.Models.DTOs;

namespace Krepim.Mobile.Features.Profile.Models.Exchange
{
    public record UpdateProfilePayload(string Email, string? PhoneNumber, AddressDto? DefaultAddress);
}
