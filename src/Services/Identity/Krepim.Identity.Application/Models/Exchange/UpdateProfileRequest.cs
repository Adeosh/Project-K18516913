using Krepim.Identity.Application.Models.DTOs;

namespace Krepim.Identity.Application.Models.Exchange
{
    public record UpdateProfileRequest(string Email, string? PhoneNumber, AddressDto? DefaultAddress);
}
