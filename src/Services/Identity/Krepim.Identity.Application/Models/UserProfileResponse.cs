namespace Krepim.Identity.Application.Models
{
    public record UserProfileResponse(
        string Email,
        string Role,
        string? PhoneNumber,
        AddressModel? DefaultAddress);
}
